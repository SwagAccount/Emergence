using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SortOrderOffset : MonoBehaviour {


    [Tooltip( "Adds the sorting order of this gameObject and it's children by this amount" )]
    [FormerlySerializedAs( "offset" )]
    [SerializeField]
    int m_offset = 1;

    /// <summary>
    /// Adds the sorting order of this gameObject and it's children by this amount
    /// </summary>
    public int offset {
        get => m_offset;
        set {
            m_offset = value;
            if (!resolveOnLateUpdate) Resolve( false );
        }
    }

    [Tooltip( "The parent canvas of this game object. If null, will be resolved" )]
    public Canvas parentCanvas;

#if UNITY_EDITOR
    [Tooltip( "Resolves the sorting order in the editor during OnValidate phase" )] [SerializeField]
    bool resolveInEditor = true;
#endif

    [Tooltip( "Resolves the sorting order on Start" )] [SerializeField]
    bool resolveOnStart = true;

    [Tooltip( "Resolves the sorting order on LateUpdate" )] [SerializeField]
    bool resolveOnLateUpdate = false;

    [SerializeField] Canvas _selfCanvas;

    void Start() {
        if (resolveOnStart) Resolve();
    }

    void OnDestroy() {
        if (_selfCanvas) {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorApplication.delayCall += () => DestroyImmediate( _selfCanvas );
            else
#endif
                Destroy( _selfCanvas );
        }
    }

#if UNITY_EDITOR
    void OnValidate() => Resolve();
#endif

    void LateUpdate() {
        if (resolveOnLateUpdate) Resolve();
    }

    public void Resolve(bool forceUpdateParent = true) {
        if (!parentCanvas || forceUpdateParent) {
            parentCanvas = transform.parent.GetComponentInParent<Canvas>();
            if (parentCanvas == null) return; // during assembly reload, this will bug out and return null
        }

        if (!_selfCanvas) {
            _selfCanvas = GetOrAddComponent<Canvas>( this );
            _selfCanvas.hideFlags = HideFlags.HideInInspector;
            _selfCanvas.overrideSorting = true;
        }

        _selfCanvas.sortingOrder = parentCanvas.sortingOrder + m_offset;
    }
    
    static T GetOrAddComponent<T>(Component component) where T : Component {
        return component.TryGetComponent<T>( out var t ) ? t : component.gameObject.AddComponent<T>();
    }
}