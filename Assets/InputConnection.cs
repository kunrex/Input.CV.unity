using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;

public class InputConnection : MonoBehaviour
{ 
    public static InputConnection Instance { get; private set; }
    
    private readonly string localhost = "127.0.0.1";
    private readonly int port = 1800;
    
    public delegate void OnDelta(Vector3 delta);
    public delegate void OnLabelChange(string newLabel);

    public delegate void OnScale(float scale);
    public delegate void OnAngles(float yaw, float pitch, float roll);
    
    public OnDelta DeltaEvent { get; set; }
    public OnScale ScaleEvent { get; set; }
    public OnAngles AngleEvent { get; set; }
    public OnLabelChange LabelEvent { get; set; }

    private string label;
    public string Label { get => label; }
    
    async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        await ManageConnection();
    }

    private async Task ManageConnection()
    {
        var client = new TcpClient(localhost, port);
        Debug.Log("Connected to python...");
            
        var stream = client.GetStream();
        try
        {
            while (true)
            {
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes > 0)
                    {
                        float[] floatData = new float[7];
                        using (var memoryStream = new MemoryStream(buffer, 0, bytes))
                        {
                            using (var reader = new BinaryReader(memoryStream))
                            {
                                for (int i = 0; i < 7; i++)
                                    floatData[i] = reader.ReadSingle();

                                var newLabel = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                                if (newLabel != label)
                                {
                                    LabelEvent(newLabel);
                                    label = newLabel;
                                }
                            }
                        }
                        
                        AngleEvent(floatData[0], floatData[1], floatData[2]);
                        DeltaEvent(new Vector3(floatData[3], floatData[4], floatData[5]));
                        ScaleEvent(floatData[6]);
                    }
                    else
                        break;
                }
                else
                    await Task.Delay(10);
            }
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
        finally
        {
            stream.Close();
            client.Close();
        }
    }
}
