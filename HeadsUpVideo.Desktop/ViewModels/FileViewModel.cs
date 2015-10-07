using HeadsUpVideo.Desktop.Models;

namespace HeadsUpVideo.Desktop.ViewModels
{
    public class FileViewModel : FileModel
    {
        public FileViewModel(FileModel model)
        {
            ContentType = model.ContentType;
            Name = model.Name;
            Path = model.Path;
            Stream = model.Stream;
        }

        public FileViewModel() { }
    }
}