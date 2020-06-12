using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static HostGame;

public class Data : MonoBehaviour
{
    public static void SaveProfile(ProfileData data)
    {
        try
        {
            string path = Application.persistentDataPath + "/profile.data";
            if (File.Exists(path)) File.Delete(path);
            FileStream file = File.Create(path);
            BinaryFormatter binary = new BinaryFormatter();
            binary.Serialize(file, data);
            Debug.Log("SAVED SUCCESSFULY");
            file.Close();
        }
        catch
        {
            Debug.Log("Error");
        }
    }
    public static ProfileData LoadProfile()
    {
        ProfileData ret = new ProfileData();
        try
        {
            string path = Application.persistentDataPath + "/profile.data";
            if (File.Exists(path))
            {
                FileStream file = File.Open(path, FileMode.Open);
                BinaryFormatter binary = new BinaryFormatter();
                ret = (ProfileData)binary.Deserialize(file);
                Debug.Log("SAVED SUCCESSFULY");
            }
        }
        catch
        {
            Debug.Log("File wasnt found");
        }
        return ret;
    }
}
