using BS_Utils.Gameplay;
using HMUI;
using System.Collections;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using Zenject;

namespace NotesPredictor
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class NotesPredictorController : MonoBehaviour
    {
        //public static NotesPredictorController Instance { get; private set; }
        private readonly float mapTime = 0f;
        private readonly CurvedTextMeshPro textMesh = new GameObject("Text").AddComponent<CurvedTextMeshPro>();
        private readonly GameObject cubeparent = new GameObject("cubeparent");
        private GameObject pair;
        private BeatmapObjectManager _manager;
        private IAudioTimeSource _audioTimeSource;

        [Inject]
        private void Constractor(BeatmapObjectManager manager, IAudioTimeSource audioTimeSource)
        {
            this._manager = manager;
            this._audioTimeSource = audioTimeSource;
            this._manager.noteWasSpawnedEvent += this.Manager_noteWasSpawnedEvent;
        }

        private void Manager_noteWasSpawnedEvent(NoteController noteController)
        {
            // TODO:複製は激重なのでMemoryPoolを実装する。
            var notePair = Instantiate(this.pair);
            var noteCube = notePair.transform.GetChild(0).gameObject;
            var noteStick = notePair.transform.GetChild(1).gameObject;
            //GameObject noteCube = Instantiate(cube);
            float x = 0;
            switch ((int)noteController.noteData.lineIndex) {
                case 0:
                    x = -0.9f;
                    break;
                case 1:
                    x = -0.3f;
                    break;
                case 2:
                    x = 0.3f;
                    break;
                case 3:
                    x = 0.9f;
                    break;
                default:
                    Destroy(notePair);
                    break;
            }
            float y = 0;
            switch (noteController.noteData.noteLineLayer) {
                case NoteLineLayer.Base:
                    y = 0.6f;
                    break;
                case NoteLineLayer.Upper:
                    y = 1.2f;
                    break;
                case NoteLineLayer.Top:
                    y = 1.8f;
                    break;
                default:
                    Destroy(notePair);
                    break;
            }
            Plugin.Log.Debug("x" + x.ToString());
            Plugin.Log.Debug("y" + y.ToString());
            float rz = 0;
            switch (noteController.noteData.cutDirection) {
                case NoteCutDirection.Down:
                    rz = 0;
                    break;
                case NoteCutDirection.Up:
                    rz = 180;
                    break;
                case NoteCutDirection.Right:
                    rz = 90;
                    break;
                case NoteCutDirection.Left:
                    rz = -90;
                    break;
                case NoteCutDirection.DownLeft:
                    rz = -45;
                    break;
                case NoteCutDirection.DownRight:
                    rz = 45;
                    break;
                case NoteCutDirection.UpLeft:
                    rz = -135;
                    break;
                case NoteCutDirection.UpRight:
                    rz = 135;
                    break;
                default:
                    noteStick = notePair.transform.GetChild(1).gameObject;
                    noteStick.SetActive(false);
                    rz = 0;
                    break;
            }
            switch (noteController.noteData.colorType) {
                case ColorType.ColorA:
                    noteCube.GetComponent<Renderer>().material.color = Color.red;
                    noteStick.GetComponent<Renderer>().material.color = new Color(1f, .3f, .3f);
                    break;
                case ColorType.ColorB:
                    noteCube.GetComponent<Renderer>().material.color = Color.blue;
                    noteStick.GetComponent<Renderer>().material.color = new Color(.3f, .3f, 1f);
                    break;
                default:
                    Destroy(noteCube);
                    break;
            }
            noteCube.transform.localPosition = new Vector3(0, 0, 0);
            noteCube.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            noteStick.transform.localPosition = new Vector3(0, .4f, 0);
            noteStick.transform.localScale = new Vector3(0.02f, .4f, 0.02f);
            notePair.transform.position = new Vector3(x, y, 1.5f);
            notePair.transform.eulerAngles = new Vector3(0, 0, rz);
            notePair.transform.SetParent(this.cubeparent.transform);
            Destroy(notePair, 1.25f);
        }

        private IEnumerator InitCoroutine()
        {
            Material noGlowMat = null;
            while (noGlowMat == null) {
                noGlowMat = Resources.FindObjectsOfTypeAll<Material>().Where(m => m.name == "UINoGlow").FirstOrDefault();
                yield return null;
            }

            this.gameObject.AddComponent<Canvas>();
            this.gameObject.AddComponent<CurvedCanvasSettings>().SetRadius(0f);

            this.pair = new GameObject("pair");
            this.pair.transform.SetParent(this.transform);
            this.pair.transform.localPosition = new Vector3(0, .5f, -1f);
            this.pair.SetActive(false);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var renderer = cube.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/SimpleLit"));
            renderer.material.color = Color.magenta;
            cube.name = "cube";
            cube.transform.SetParent(this.pair.transform);
            cube.transform.localPosition = new Vector3(0, 0, 0);
            cube.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            cube.SetActive(true);
            var stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            renderer = stick.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Custom/SimpleLit"));
            renderer.material.color = Color.white;
            stick.name = "stick";
            stick.transform.SetParent(this.pair.transform);
            stick.transform.localPosition = new Vector3(0, .27f, 0);
            stick.transform.localScale = new Vector3(0.01f, .4f, 0.01f);
            stick.SetActive(true);
        }
        // These methods are automatically called by Unity, you should remove any you aren't using.
        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            // For this particular MonoBehaviour, we only want one instance to exist at any time, so store a reference to it in a static property
            //   and destroy any that are created while one already exists.
            //if (Instance != null) {
            //    Plugin.Log?.Warn($"Instance of {GetType().Name} already exists, destroying.");
            //    GameObject.DestroyImmediate(this);
            //    return;
            //}
            //GameObject.DontDestroyOnLoad(this); // Don't destroy this object on scene changes
            //Instance = this;
            Plugin.Log?.Debug($"{this.name}: Awake()");
            // スコアの送信無効化（一応）
            ScoreSubmission.DisableSubmission(Assembly.GetExecutingAssembly().GetName().Name);
        }   
        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            //text
            this.gameObject.AddComponent<Canvas>();
            this.textMesh.transform.SetParent(this.transform);
            this.textMesh.alignment = TextAlignmentOptions.Center;
            this.textMesh.transform.eulerAngles = new Vector3(0, 0, 0);
            this.textMesh.transform.position = new Vector3(0, 2f, 3f);
            this.textMesh.color = Color.white;
            this.textMesh.fontSize = 0.2f;
            this.textMesh.text = "";
            this.StartCoroutine(this.InitCoroutine());
        }

        /// <summary>
        /// フレームレートによらないのでHMDのフレッシュレートが変わっても大丈夫な気がするやつ
        /// </summary>
        private void FixedUpdate()
        {
            // 毎フレームforを回すのは負荷的にどうなのか心配な感じはする。
            for (var i = 0; i < this.cubeparent.transform.childCount; i++) {
                var notePair = this.cubeparent.transform.GetChild(i)?.gameObject;
                var noteCube = notePair.transform.GetChild(0)?.gameObject;
                if (!notePair || !noteCube) {
                    break;
                }
                notePair.SetActive(true);
                var nowscale = noteCube.transform.localScale;
                nowscale *= 1.14f;
                noteCube.transform.localScale = nowscale;
            }
        }
        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Plugin.Log?.Debug($"{this.name}: OnDestroy()");
            this._manager.noteWasSpawnedEvent -= this.Manager_noteWasSpawnedEvent;
            //if (Instance == this)
            //    Instance = null; // This MonoBehaviour is being destroyed, so set the static instance property to null.
        }
        #endregion
    }
}