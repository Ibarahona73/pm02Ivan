using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pm02Ivan.Models;
using Firebase.Database;
using Firebase.Database.Query;

namespace pm02Ivan.Controllers
{
    public class firebaseController
    {
        //Cliente de Firebase
        private FirebaseClient client = new FirebaseClient("https://pm02ivan-default-rtdb.firebaseio.com/");

        //Constructor Vacio
        public firebaseController() { }

        //Metodo para crear/agregar/update un nuevo Recordatorio
        public async Task<bool> CrearProducto(FireProd recordatorio)
        {
            if (string.IsNullOrEmpty(recordatorio.Key))
            {
                try
                {
                    Console.WriteLine("Intentando crear un nuevo Recordatorio...");
                    var recordatorios = await client.Child("Recordatorio").OnceAsync<FireProd>();
                    Console.WriteLine($"Número de Recordatorios existentes: {recordatorios.Count}");
                    if (recordatorios.Count == 0 || recordatorio!=null)
                    {
                        Console.WriteLine("No hay Recordatorios existentes. Creando un nuevo Recordatorio...");
                        await client.Child("Recordatorio").PostAsync(new FireProd
                        {
                            Id_nota = recordatorio.Id_nota,
                            Desc = recordatorio.Desc,
                            Fecha = recordatorio.Fecha,
                            Foto = recordatorio.Foto,
                            Audio = recordatorio.Audio,
                        });

                        Console.WriteLine("Recordatorio creado con éxito.");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Ya existen Recordatorios en la base de datos.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar crear un nuevo Recordatorio: {ex.Message}");
                    return false;
                }
            }
            else
            {
                try
                {
                    Console.WriteLine("Intentando actualizar un Recordatorio existente...");
                    await client.Child("Recordatorio").Child(recordatorio.Key).PutAsync(recordatorio);
                    Console.WriteLine("Recordatorio actualizado con éxito.");
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al intentar actualizar el Recordatorio: {ex.Message}");
                    return false;
                }
            }

            return false;
        }

        //Metodo para Lectura de elementos
        public async Task<List<FireProd>> GetListProductos()
        {
            var productos = await client.Child("Recordatorio").OnceSingleAsync<Dictionary<string, FireProd>>();

            return productos.Select(x => new FireProd
            {
                Key = x.Key,
                Id_nota = x.Value.Id_nota,
                Desc = x.Value.Desc,
                Fecha = x.Value.Fecha,
                Foto = x.Value.Foto,
                Audio = x.Value.Audio,
            }).ToList();
        }

        //Delete
        public async Task<bool> deleteProducto(string key)
        {
            try
            {
                await client.Child("Recordatorio").Child(key).DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}