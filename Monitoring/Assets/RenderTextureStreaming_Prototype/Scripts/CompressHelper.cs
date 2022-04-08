using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;


// Start is called before the first frame update
public static class CompressHelper {
    
    public static byte[] Compress(byte[] data)
    {
        MemoryStream output = new MemoryStream();
        using (DeflateStream dstream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            dstream.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    public static byte[] Decompress(byte[] data)
    {
        byte[] decompressedArray = null;
        try
        {
            using (MemoryStream decompressedStream = new MemoryStream())
            {
                using (MemoryStream compressStream = new MemoryStream(data))
                {
                    using (DeflateStream deflateStream = new DeflateStream(compressStream, CompressionMode.Decompress))
                    {
                        deflateStream.CopyTo(decompressedStream);
                    }
                }
                decompressedArray = decompressedStream.ToArray();
            }
        }
        catch (Exception exception)
        {
            // do something !
            Debug.Log("동일한곳에서는 안됨...");
        }

        return decompressedArray;
    }
   
}