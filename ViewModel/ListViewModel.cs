using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pm02Ivan.Controllers;
using pm02Ivan.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Plugin.Maui.Audio;
using Android.Media;


namespace pm02Ivan.ViewModel
{
    public class ListViewModel : BaseView
    {
        private bool _orderByDescending = true;
        private ObservableCollection<Models.FireProd> _memorias;
        private firebaseController firebaseControl = new firebaseController();


        public ObservableCollection<Models.FireProd> memo
        {
            get { return _memorias; }
            set { _memorias = value; OnPropertyChanged(); }
        }

        private Models.FireProd _selectedmemory;

        public Models.FireProd SelectedMemory
        {
            get { return _selectedmemory; }
            set { _selectedmemory = value; OnPropertyChanged(); }
        }

        public bool OrderByDescending
        {
            get { return _orderByDescending; }
            set { _orderByDescending = value; OnPropertyChanged(); }
        }

        public ICommand GoToDetailsCommand { private set; get; }

        public ICommand DeleteCommand { private set; get; }

        public ICommand NuevoProductoCommand { private set; get; }

        public INavigation Navigation { get; set; }
        public ICommand SalirCommand { private set; get; }
        public ICommand HearCommand { private set; get; }

        public ICommand ChangeOrderByCommand => new Command(() =>
        {
            OrderByDescending = !OrderByDescending;
            loadProductos();
        });

        public ListViewModel(INavigation navigation)
        {
            Navigation = navigation;
            GoToDetailsCommand = new Command<Type>(async (pageType) => await GoToDetails(pageType, SelectedMemory));
            NuevoProductoCommand = new Command<Type>(async (pageType) => await NuevoProducto(pageType));
            DeleteCommand = new Command(async () => await DeleteProducto(SelectedMemory.Key));
            HearCommand = new Command(async () => await Escuchar(SelectedMemory.Key));           

            loadProductos();
        }

        async Task loadProductos()
        {
            List<FireProd> listProductos;

            memo = new ObservableCollection<FireProd>();

            try
            {
                listProductos = await firebaseControl.GetListProductos();

                if (OrderByDescending)
                {
                    listProductos = listProductos.OrderByDescending(p => p.Fecha).ToList();
                }
                else
                {
                    listProductos = listProductos.OrderBy(p => p.Fecha).ToList();
                }

                foreach (var product in listProductos)
                {
                    FireProd productos = new FireProd
                    {
                        Key = product.Key,
                        Id_nota = product.Id_nota,
                        Desc = product.Desc,
                        Fecha = product.Fecha,
                        Foto = product.Foto,
                        Audio = product.Audio,
                    };

                    memo.Add(productos);
                }

            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Atención", "Se produjo un error al obtener los Recordatorios", "OK");
            }
        }


        async Task GoToDetails(Type pageType, FireProd selectedMemory)
        {
            if (selectedMemory != null)
            {

                var page = (Page)Activator.CreateInstance(pageType);

                var viewModel = new MemoryViewModel();
                viewModel.SelectedMemory = selectedMemory;
                viewModel.Fecha = selectedMemory.Fecha;
                viewModel.Desc = selectedMemory.Desc;
                viewModel.Foto = selectedMemory.Foto;

                // Verificar si el audio no es nulo antes de pasarlo
                if (!string.IsNullOrEmpty(selectedMemory.Audio))
                {
                    viewModel.Audio = selectedMemory.Audio;
                    // Reproducir el audio
                    await Escuchar(selectedMemory.Key);
                }

                viewModel.Key = selectedMemory.Key;
                page.BindingContext = viewModel;

                await Navigation.PushAsync(page);
            }
        }

        async Task NuevoProducto(Type pageType)
        {
            var page = (Page)Activator.CreateInstance(pageType);

            var viewModel = new MemoryViewModel();
            viewModel.SelectedMemory = null;
            page.BindingContext = viewModel;
            await Navigation.PushAsync(page);
        }

        async Task DeleteProducto(string key)
        {
            if (SelectedMemory != null)
            {
                var tappedItem = memo.FirstOrDefault(item => item.Key == key);
                bool userConfirmed = await Application.Current.MainPage.DisplayAlert("Confirmación", "¿Está seguro de que desea eliminar este Recordatorio?", "Si", "No");
                if (userConfirmed)
                {
                    try
                    {

                        if (firebaseControl != null)
                        {
                            bool success = await firebaseControl.deleteProducto(key);

                            if (success)
                            {
                                memo.Remove(tappedItem);
                                SelectedMemory = null;

                                await Application.Current.MainPage.DisplayAlert("Atención", "Recordatorio Eliminado", "OK");
                            }
                        }

                    }
                    catch
                    {
                        await Application.Current.MainPage.DisplayAlert("Atención", "No se pudo eliminar el Recordatorio", "OK");
                    }
                }

            }

        }
        async Task Escuchar(string key)
        {
            if (SelectedMemory != null)
            {
                var tappedItem = memo.FirstOrDefault(item => item.Key == key);

                try
                {
                    // Obtener el audio en formato base64 del elemento seleccionado
                    string audioBase64 = tappedItem.Audio;

                    // Convertir el audio de base64 a bytes
                    byte[] audioBytes = Convert.FromBase64String(audioBase64);

                    // Crear un archivo temporal para el audio
                    string tempFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp_audio.mp3");

                    // Guardar los bytes del audio en el archivo temporal
                    File.WriteAllBytes(tempFilePath, audioBytes);

                    // Reproducir el audio
                    MediaPlayer mediaPlayer = new MediaPlayer();
                    mediaPlayer.SetDataSource(tempFilePath);
                    mediaPlayer.Prepare();
                    mediaPlayer.Start();
                }
                catch (Exception ex)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo reproducir el audio.", "OK");
                }
            }
        }
    }
}