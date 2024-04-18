using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Firebase.Database;
using Firebase.Database.Query;
using Plugin.AudioRecorder;
using pm02Ivan.Controllers;


namespace pm02Ivan.ViewModel
{
    public class MemoryViewModel : BaseView
    {
        bool isRecording = false;
        FirebaseClient client = new FirebaseClient("https://pm02ivan-default-rtdb.firebaseio.com/");
        private firebaseController Firebasecontrol = new firebaseController();        
        private string productKey;
        private DateTime _Fecha;
        private int _Id_nota;
        private string _Desc;
        private string _foto;
        private string _audio;
        private string _key;
        private bool _visibilityCreate;
        private bool _visibilityUpdate;
        private Models.FireProd _selectedMemory;

        AudioRecorderService recorder = new AudioRecorderService();
        Plugin.AudioRecorder.AudioPlayer players;
        string filePath;
        byte[] audi;

        

        public string Key
        {
            get { return _key; }
            set { _key = value; OnPropertyChanged(); }
        }


        public int Id_nota
        {
            get { return _Id_nota; }
            set { _Id_nota = value; OnPropertyChanged();}
        }

        public DateTime Fecha
        {
            get { return _Fecha; }
            set { _Fecha = value; OnPropertyChanged(); }
        }

        public string Desc
        {
            get { return _Desc; }
            set { _Desc = value; OnPropertyChanged(); }
        }

        public string Foto
        {
            get { return _foto; }
            set { _foto = value; OnPropertyChanged(); }
        }

        public string Audio
        {
            get { return _audio; }
            set { _audio = value; OnPropertyChanged(); }
        }

        public bool VisibilityCreate
        {
            get { return _visibilityCreate; }
            set { _visibilityCreate = value; OnPropertyChanged(); }
        }

        public bool VisibilityUpdate
        {
            get { return _visibilityUpdate; }
            set { _visibilityUpdate = value; OnPropertyChanged(); }

        }


        public Models.FireProd SelectedMemory
        {
            get { return _selectedMemory; }
            set
            {
                _selectedMemory = value;
                OnPropertyChanged();

                Console.WriteLine($"SelectedProduct changed to: {_selectedMemory?.Desc}");

                ShowProductStatusAlert();
            }
        }

        public MemoryViewModel()
        {
            CleanCommand = new Command(Cleaner);
            FotoCommand = new Command(() => TomarFoto());
            AudioCommand = new Command(() => GrabarAudio());
            CreateCommand = new Command(async () => await CreateData());
            UpdateCommand = new Command(async () => await UpdateProducto(productKey));
        }

        public ICommand CleanCommand { get; private set; }
        public ICommand CreateCommand { get; private set; }
        public ICommand ReadCommand { get; private set; }
        public ICommand UpdateCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand FotoCommand { get; private set; }
        public ICommand AudioCommand { get; private set; }


        private void Cleaner()
        {
            Id_nota = 0;
            Desc = string.Empty;
            Fecha = DateTime.Today;
            Foto = string.Empty;
           
        }

        async void ShowProductStatusAlert()
        {
            if (SelectedMemory != null)
            {
                productKey = SelectedMemory.Key;
                VisibilityCreate = false;
                VisibilityUpdate = true;
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Instrucciones", "Pasos Para Grabar \n\n1.Click En El Boton Cafe Para Empezar.\n\n2.Tendras 10s De Grabacion y se Detendra Automaticamente.\n\n" +
                    "3.Si Quieres Escuchar Lo Que Grabaste , Da Click Nuevamente En El Boton.\n\n4.Para Reemplazar el Audio Da Click Nuevamente En El Boton.\n\nLuego De Escuchar Lo Que " +
                    "Grabaste y Dar Click En El Boton la Grabacion Se Reiniciara, Borrando La Anterior.", "Entendido");
                VisibilityCreate = true;
                VisibilityUpdate = false;
            }
        }





                                                                                                               //CRUD 
    
        
        

    async Task CreateData()
    {       

        if (Fecha.Date <= DateTime.Today)
        {
        await Application.Current.MainPage.DisplayAlert("Atención", "Por favor Cambiar Fecha del Recordatorio a un dia que no sea el de hoy o Fechas Anteriores", "OK");
        return;
        }
        else if (string.IsNullOrEmpty(Desc))
        {
            await Application.Current.MainPage.DisplayAlert("Atención", "Por favor ingrese una descripcion para el recordatorio", "OK");
            return;
        }

        try
        {
            var product = new Models.FireProd
            {                
                Desc = Desc,
                Fecha = Fecha,
                Foto = Foto,
                Audio = Convert.ToBase64String(audi),
            };

            if (Firebasecontrol != null)
            {
                bool addedSuccessfully = await Firebasecontrol.CrearProducto(product);

                if (addedSuccessfully)
                {
                    await Application.Current.MainPage.DisplayAlert("Atención", "Recordatorio Creado", "OK");
                    var navigation = Application.Current.MainPage.Navigation;
                    await navigation.PushAsync(new Views.PageListMemory());
                }
            }

        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Atención", "No se pudo crear el Recordatorio", "OK");
        }
    }

    async Task UpdateProducto(string key)
    {            

        if (Fecha.Date <= DateTime.Today)
        {
            await Application.Current.MainPage.DisplayAlert("Atención", "Por favor Cambiar Fecha del Recordatorio a un dia que no sea el de hoy o Fechas Anteriores", "OK");
            return;
        }
        else if (string.IsNullOrEmpty(Desc))
        {
            await Application.Current.MainPage.DisplayAlert("Atención", "Por favor Ingresar un comentario al recordatorio", "OK");
            return;
        }

        try
        {
            var product = new Models.FireProd
            {
                Key = key,               
                Fecha = Fecha,
                Desc= Desc,
                Foto = Foto,
                Audio = Convert.ToBase64String(audi),
            };

            if (Firebasecontrol != null)
            {
                bool addedSuccessfully = await Firebasecontrol.CrearProducto(product);

                if (addedSuccessfully)
                {
                    await Application.Current.MainPage.DisplayAlert("Atención", "Recordatorio Actualizado", "OK");
                    var navigation = Application.Current.MainPage.Navigation;
                    await navigation.PushAsync(new Views.PageListMemory());
                }
            }

        }
        catch
        {
            await Application.Current.MainPage.DisplayAlert("Atención", "No se pudo actualizar el Recordatorio", "OK");
        }
    }

    //Tomar Foto
    async void TomarFoto()
    {
        FileResult photo = await MediaPicker.CapturePhotoAsync();

        if (photo != null)
        {
            string photoPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
            using (Stream sourcephoto = await photo.OpenReadAsync())
            using (FileStream streamlocal = File.OpenWrite(photoPath))
            {
                await sourcephoto.CopyToAsync(streamlocal);

                Foto = Convertir.PhotoHelper.GetImg64(photo);
            }
        }
    }

        async void GrabarAudio()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Microphone>();

            recorder.TotalAudioTimeout = TimeSpan.FromSeconds(3600);
            recorder.StopRecordingOnSilence = false;
            players = new Plugin.AudioRecorder.AudioPlayer();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Microphone>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert("Permiso Requerido", "Se requieren permisos de micrófono para grabar audio.", "Aceptar");
                    return;
                }
            }
            else if (!isRecording)
            {

                // Si no se está grabando, iniciar grabación
                recorder.TotalAudioTimeout = TimeSpan.FromSeconds(10);
                recorder.StopRecordingOnSilence = false;
                await recorder.StartRecording();
                isRecording = true; // Cambiar estado a grabando

            }

            else
            {
                // Si ya se está grabando, detener grabación
                await recorder.StopRecording();
                isRecording = false; // Cambiar estado a no grabando
                filePath = recorder.GetAudioFilePath();
                audi = ConvertAudioToBase64(filePath);
                players.Play(filePath);
            }
        }
        private byte[] ConvertAudioToBase64(string filePath)
        {
            byte[] audio = File.ReadAllBytes(filePath);
            return audio;
        }

    }

}

