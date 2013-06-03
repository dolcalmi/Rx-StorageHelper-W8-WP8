using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Windows.Storage.Streams;

namespace DCM.Helpers
{
  /// <summary>
  /// Reactive storage helper
  /// created by: @dolcalmi -_-
  /// </summary>
  public static class StorageHelper
  {
    public static StorageFolder defaultFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

    #region "Write methods"

    /// <summary>
    /// Save object to default folder in xml format
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="data">object to save</param>
    /// <returns></returns>
    public static IObservable<bool> SaveDataObservable(string fileName, object data)
    {
      return SaveDataObservable(fileName, defaultFolder, data);
    }

    /// <summary>
    /// Save object to specified folder in xml format
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="folder">Folder that will contains the file</param>
    /// <param name="data">object to save</param>
    /// <returns></returns>
    public static IObservable<bool> SaveDataObservable(string fileName, StorageFolder folder, object data)
    {
      return SaveData(fileName, folder, data).ToObservable();
    }

    /// <summary>
    /// Save stream to default folder
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="data">object to save</param>
    /// <returns></returns>
    public static IObservable<bool> SaveDataStreamObservable(string fileName, Stream data)
    {
      return SaveDataStreamObservable(fileName, defaultFolder, data);
    }

    /// <summary>
    /// Save stream to specified folder
    /// </summary>
    /// <param name="fileName">File name</param>
    /// <param name="folder">Folder that will contains the file</param>
    /// <param name="data">object to save</param>
    /// <returns></returns>
    public static IObservable<bool> SaveDataStreamObservable(string fileName, StorageFolder folder, Stream data)
    {
      return SaveDataStream(fileName, folder, data).ToObservable();
    }

    /// <summary>
    /// Create folder in default location
    /// </summary>
    /// <param name="newFolder">Folder name</param>
    /// <returns></returns>
    public static IObservable<StorageFolder> CreateFolderObservable(string newFolder)
    {
      return CreateFolderObservable(newFolder, defaultFolder);
    }

    /// <summary>
    /// Create folder in specified location
    /// </summary>
    /// <param name="newFolder">Folder name</param>
    /// <param name="folder">Folder container</param>
    /// <returns></returns>
    public static IObservable<StorageFolder> CreateFolderObservable(string newFolder, StorageFolder folder)
    {
      return CreateFolder(newFolder, folder).ToObservable();
    }

    public async static Task<StorageFolder> CreateFolder(string newFolder, StorageFolder folder)
    {
      var option = Windows.Storage.CreationCollisionOption.OpenIfExists;
      return await folder.CreateFolderAsync(newFolder, option);
    }

    public async static Task<bool> SaveData(string fileName, object data)
    {
      return await SaveData(fileName, defaultFolder, data);
    }

    public async static Task<bool> SaveData(string fileName, StorageFolder folder, object data)
    {
      using (MemoryStream saveData = new MemoryStream())
      {
        XmlSerializer x = new XmlSerializer(data.GetType());
        x.Serialize(saveData, data);

        return await SaveDataStream(fileName, folder, saveData);
      }
    }

    public async static Task<bool> SaveDataStream(string fileName, Stream data)
    {
      return await SaveDataStream(fileName, defaultFolder, data);
    }

    public async static Task<bool> SaveDataStream(string fileName, StorageFolder folder, Stream data)
    {
      if (string.IsNullOrWhiteSpace(fileName) || data == null)
        return false;

      // settings
      var option = Windows.Storage.CreationCollisionOption.ReplaceExisting;

      try
      {
        folder = folder ?? defaultFolder;
        var file = await folder.CreateFileAsync(fileName, option);

        if (data.Length > 0)
        {

          // Get an output stream for the SessionState file and write the state asynchronously
          using (var fileStream = await file.OpenStreamForWriteAsync())
          {
            data.Seek(0, SeekOrigin.Begin);
            await data.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
          }
          return true;
        }
      }
      catch (Exception ex)
      {
        System.Diagnostics.Debug.WriteLine(ex.Message);
      }

      return false;
    }

    #endregion

    #region "Read methods"
    
    /// <summary>
    /// Read object from xml file
    /// </summary>
    /// <typeparam name="T">Type to cast file</typeparam>
    /// <param name="fileName">File name</param>
    /// <returns></returns>
    public static IObservable<T> LoadDataObservable<T>(string fileName) where T : class
    {
      return LoadDataObservable<T>(fileName, defaultFolder);
    }

    /// <summary>
    /// Read object from xml file
    /// </summary>
    /// <typeparam name="T">Type to cast file</typeparam>
    /// <param name="fileName">File name</param>
    /// <param name="folder">Folder that contains the file</param>
    /// <returns></returns>
    public static IObservable<T> LoadDataObservable<T>(string fileName, StorageFolder folder) where T : class
    {
      return LoadData<T>(fileName, folder).ToObservable();
    }

    /// <summary>
    /// Load a list of objects from folder files. Folder is located in defatult folder
    /// </summary>
    /// <typeparam name="T">Type to cast files</typeparam>
    /// <param name="folderName">Folder that contains the files</param>
    /// <returns></returns>
    public static IObservable<List<T>> LoadDataListObservable<T>(string folderName) where T : class
    {
      return LoadDataList<T>(folderName, defaultFolder).ToObservable();
    }

    /// <summary>
    /// Load a list of objects from folder files. Folder is located in specified folder.
    /// </summary>
    /// <typeparam name="T">Type to cast files</typeparam>
    /// <param name="folderName">Folder that contains the files</param>
    /// <param name="folder">Folder that</param>
    /// <returns></returns>
    public static IObservable<List<T>> LoadDataListObservable<T>(string folderName, StorageFolder folder) where T : class
    {
      return LoadDataList<T>(folderName, folder).ToObservable();
    }

    public async static Task<T> LoadData<T>(string fileName) where T : class
    {
      return await LoadData<T>(fileName, defaultFolder);
    }

    public async static Task<T> LoadData<T>(string fileName, StorageFolder folder) where T : class
    {
      try
      {
        var file = await folder.GetFileAsync(fileName);
        var f = await LoadData<T>(file);
        return f;
      }
      catch { return null; }
    }

    public async static Task<List<T>> LoadDataList<T>(string folderName, StorageFolder folder) where T : class
    {
      try
      {
        folder = folder ?? defaultFolder;
        var f = string.IsNullOrWhiteSpace(folderName) ? folder : await folder.GetFolderAsync(folderName);

        List<T> files = new List<T>();
        T obj;

        foreach (var file in await f.GetFilesAsync())
        {
          obj = await LoadData<T>(file);
          if (obj != null)
            files.Add(obj);
        }

        return files;
      }
      catch { return null; }
    }

    private async static Task<T> LoadData<T>(StorageFile file) where T : class
    {
      using (IInputStream inStream = await file.OpenSequentialReadAsync())
      {
        try
        {
          // Deserialize the Session State
          XmlSerializer x = new XmlSerializer(typeof(T));
          return x.Deserialize(inStream.AsStreamForRead()) as T;
        }
        catch (Exception)
        {
          return null;
        }
      }
    }

    #endregion

    #region "Delete methods"
    
    /// <summary>
    /// Delete file from specified folder
    /// </summary>
    /// <param name="folder">Folder that contains the file</param>
    /// <param name="filename">File name</param>
    /// <returns>Throws exception if file can't be deleted</returns>
    public static IObservable<bool> DeleteFile(StorageFolder folder, string filename)
    {
      var source = from file in folder.GetFileAsync(filename).AsTask().ToObservable()
                   from result in file.DeleteAsync().AsTask().ToObservable()
                   select true;

      return source;
    }

    /// <summary>
    /// Delete file from specified folder without throw exception if something goes wrong.
    /// </summary>
    /// <param name="folder">Folder that contains the file</param>
    /// <param name="filename">File name</param>
    /// <returns>False, if file can't be deleted</returns>
    public static IObservable<bool> SafeDeleteFile(StorageFolder folder, string filename)
    {
      try
      {
        return DeleteFile(folder, filename);
      }
      catch
      {
        return Observable.Return<bool>(false);
      }
    }

    #endregion
  }
}