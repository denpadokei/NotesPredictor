using SiraUtil;
using Zenject;

namespace NotesPredictor.Installers
{
    public class NPGameInstaller : MonoInstaller
    {
        public override void InstallBindings() => this.Container.BindInterfacesAndSelfTo<NotesPredictorController>().FromNewComponentOnNewGameObject().AsCached().NonLazy();
    }
}
