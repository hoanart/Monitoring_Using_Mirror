
#if UNITY_EDITOR
//#define TRACE_ON
#endif

using System;
using System.Collections;
using System.Diagnostics;
using Terra;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(NetworkIdentity))]
public class SendTexture : TextureNetworkBehaviour {

    public Camera renderCam;
    public RenderTexture rt;

    [HideInInspector]
    public int width;
    [HideInInspector]
    public int height;
    
    [SerializeField] [Tooltip("텍스쳐 바이트를 간격을 두고 보낸다. ex) 0.1일 경우, 10초당 1번")]
    private float sendInterval = 0.1f; //10초당 한번.

    [Tooltip("분할한 데이터를 모두 서버로 보냈는가?")]
    public bool isCompleteArray;
    [Tooltip("데이터를 서버로 보냈는가?")]
    public bool isSend;
    
    [Tooltip("로컬플레이어가 렌더 카메라 오브젝트를 활성화 했는가?")]
    public bool isActiveRenderCam;
    
    //텍스쳐 바이트 저장 배열.
    private NativeArray<byte> nativeBytes;

    private byte[] bytes;

    private byte[] compressedBytes;

    //maximumPacketSize를 넘는 경우 분할 바이트 배열.
    private byte[] temporaryBytes;
    private byte[] splitBytes;
    private byte[] splitRestBytes;

    //서버로 보낼 수 있는 초대 패킷사이즈.
    private int maximumPacketSize;

    private int start;
    private int end;

    private bool mbSendData = true;

    #region 클라이언트 실행중 반복

    public override void OnStartClient()
    {
        InvokeRepeating("RepeatSend", 1, sendInterval);
    }
/// <summary>
/// 렌더카메라가 켜져있으며 데이터가 압축이 되었을 때 반복해서 서버로 압축된 데이터를 보낸다.
/// </summary>
    private void RepeatSend()
    {
        if (!isActiveRenderCam)
        {
            return;
        }
        if (!mbSendData&&isClient)
        {
            if (maximumPacketSize >= compressedBytes.Length)
            {
                CmdSend(compressedBytes);
            }
            else
            {
                SendSplitBytesToServer();
            }
        }
    }
/// <summary>
/// 데이터를 분할하여 전송한다.(한번에 보낼 수 있는 최대 패킷량 : 149174 byte) 
/// </summary>
void SendSplitBytesToServer()
{
    if (start < end)
    {
        DebugLogWrap(
            $"분할 전송 시작, 총 바이트 수 : {end}, 전송 분할 시작 바이트 오프셋(start) :{start}, 남은 바이트 수(end - start) : {end - start}");
        // Debug.Log($"분할 전송 시작, 총 바이트 수 : {end}, 전송 분할 시작 바이트 오프셋(start) :{start}, 남은 바이트 수(end - start) : {end - start}");
        int size = end - start >= maximumPacketSize ? maximumPacketSize : end - start;

        if (size == maximumPacketSize)
        {
            Array.Clear(splitBytes, 0, splitBytes.Length);
            Buffer.BlockCopy(compressedBytes, start, splitBytes, 0, size);
            CmdSendSplitByte(splitBytes, start, size, end);
            start += size;
        }
        else
        {
            if (size != splitRestBytes.Length)
            {
                Array.Resize(ref splitRestBytes, size);
            }

            Array.Clear(splitRestBytes, 0, splitRestBytes.Length);
            Buffer.BlockCopy(compressedBytes, start, splitRestBytes, 0, size);
            CmdSendSplitByte(splitRestBytes, start, size, end);
            start += size;
        }
    }
}
    #endregion


    public void Awake()
    {
        nativeBytes = new NativeArray<byte>();
    }

    // Start is called before the first frame update
    void Start()
    {
        maximumPacketSize = Transport.activeTransport.GetMaxPacketSize() - 50;
        DebugLogWrap($"MaximumPacketSize : {maximumPacketSize}");
        
        splitBytes = new byte[maximumPacketSize];
        splitRestBytes = new byte[maximumPacketSize];

    }
    
    void Update()
    {
        
        if (isClient)
        {
            if (!NetworkClient.localPlayer.enabled)
            {
                return;
            }
                
            if (renderCam == null)
            {
                if (GameObject.Find("RenderCamera_Client")==null)
                {
                    return;
                }
                    renderCam = GameObject.Find("RenderCamera_Client").GetComponent<Camera>();
                    rt = renderCam.targetTexture;
                    width = rt.width;
                    height = rt.height;
                    CmdRenderTextureSize(rt.width, rt.height);
                    // rt.width = width;
                    //rt.height = height;
            }
        }
    }

    private void LateUpdate()
    {
        if (renderCam != null && source != null && isClient)
        {
            CompressData();
        }
    }

   
/// <summary>
/// 텍스처 데이터를 압축한다.
/// </summary>
    public void CompressData()
    {
        if (isActiveRenderCam)
        {
            CmdExistRenderTexture(isActiveRenderCam);
        }
        
        if (mbSendData&&isActiveRenderCam)
        {
            //Debug.Log("코루틴 진행중.EndOfFrame");
            nativeBytes = source.GetRawTextureData<byte>();

            if (bytes == null)
            {
                bytes = new byte[nativeBytes.Length];
                //CmdSetByte(bytes.Length);
            }
            else
            {
                DebugLogWrap($"mbSendData : "+mbSendData);
                mbSendData = false;
                CopyFromNativeBytes(nativeBytes, ref bytes);
                
                compressedBytes = CompressHelper.Compress(bytes);
                if (Time.frameCount % 90 == 0)
                {
                    GC.Collect();
                }
                end = compressedBytes.Length;

                if (compressedBytes.Length > maximumPacketSize)
                {
                    CmdResize(end);
                }
            }
        }
    }
    /// <summary>
    /// NativeArray의 텍스쳐 데이터를 byte Array로 변경한다.
    /// </summary>
    /// <param name="nativeArr"></param>
    /// <param name="targetArr"></param>
    [Client]
    private void CopyFromNativeBytes(NativeArray<byte> nativeArr, ref byte[] targetArr)
    {
        if (targetArr.Length != nativeArr.Length)
        {
            Array.Resize(ref targetArr, nativeArr.Length);
        }

        Array.Clear(targetArr, 0, targetArr.Length);

        var slice = new NativeSlice<byte>(nativeArr).SliceConvert<byte>();
        slice.CopyTo(targetArr);
    }

    /// <summary>
    ///  Render texture의 존재 여부를 반환한다.
    /// </summary>
    /// <returns></returns>
    public bool HasRenderTexture()
    {
        if (rt != null)
        {
            return true;
        }
        return false;
    }
/// <summary>
/// 텍스처 데이터 압축을 푼다.
/// </summary>
/// <returns></returns>
    public byte[] DisplayCompressByte()
    {
        return compressedBytes;
    }


    [Conditional("TRACE_ON")]
    public void DebugLogWrap( string str)
    {
        Debug.Log(str);
    }
    [Conditional("TRACE_ON")]
    public  void DebugAssertWrap( )
    {
        Debug.Assert(false);
    }
    
    
    #region Command

    #region In RepeatSend Method

    /// <summary>
    /// 압축된 데이터를 서버로 보낸다.
    /// </summary>
    /// <param name="bytes">압축된 데이터</param>
    [Command(requiresAuthority = false)]
    private void CmdSend(byte[] bytes)
    {
            compressedBytes = bytes;
            DebugLogWrap($"CompressedBytes : {compressedBytes.Length}");
            isSend = true;
            RpcFinish();
    }
/// <summary>
/// 압축된 데이터를 분할하여 서버로 보낸다.
/// </summary>
/// <param name="bytes">압축된 데이터</param>
/// <param name="start">복사될 곳에 대한 오프셋(0부터 시작) </param>
/// <param name="size">복사할 바이트 수</param>
/// <param name="end">총 바이트 수</param>
    [Command(requiresAuthority = false)]
    private void CmdSendSplitByte(byte[] bytes, int start, int size, int end)
    {
        if (temporaryBytes != null)
        {
            Buffer.BlockCopy(bytes, 0, temporaryBytes, start, size);
            DebugLogWrap($"받은 바이트의 길이 : {bytes.Length}, 현재 바이트 길이 : {temporaryBytes}");
            if (start + size == end)
            {
                compressedBytes = temporaryBytes;
                isSend = true;
                RpcFinish();
            }
        }
        else
        {
            DebugAssertWrap();
        }
    }

    #endregion

    #region In Coroutine
/// <summary>
/// 현재의 렌더텍스처의 사이즈를 서버로 보낸다.
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
    [Command(requiresAuthority = false)]
    void CmdRenderTextureSize(int width,int height)
    {
        this.width = width;
        this.height = height;
    }
    
    
    [Command(requiresAuthority = false)]
    void CmdExistRenderTexture(bool bExist)
    {
        isActiveRenderCam = bExist;
    }
    /// <summary>
    /// 서버에 텍스쳐의 바이트 배열 인스턴스를 만든다.
    /// </summary>
    /// <param name="length">배열의 길이</param>
    [Command(channel = Channels.Unreliable, requiresAuthority = false)]
    void CmdSetByte(int length)
    {
        bytes = new byte[length];
        DebugLogWrap($"텍스쳐 바이트 길이 : {bytes.Length}");

       // RpcFinish();
    }

    /// <summary>
    /// 서버에 나눠 보낼 바이트 배열을 인스턴스 하거나 크기를 변경한다.
    /// </summary>
    /// <param name="end">배열의 길이</param>
    [Command(channel = Channels.Unreliable, requiresAuthority = false)]
    void CmdResize(int end)
    {
        if (temporaryBytes == null)
        {
            temporaryBytes = new byte[end];
        }
        else if (temporaryBytes.Length == end)
        {
            DebugLogWrap($"splitbytes 사이즈 동일({temporaryBytes.Length})");
            Array.Clear(temporaryBytes, 0, temporaryBytes.Length);
        }
        else
        {
            Array.Resize(ref temporaryBytes, end);
            Array.Clear(temporaryBytes, 0, temporaryBytes.Length);
        }
    }

    #endregion

    #endregion

    #region Rpc

    [ClientRpc]
    void RpcFinish()
    {
        mbSendData = true;
        start = 0;
    }

    #endregion
    
    // IEnumerator CompressTexture()
    // {
    //     WaitForEndOfFrame endOfFrame = new WaitForEndOfFrame();
    //     while (true)
    //     {
    //         if (!isClient)
    //         {
    //             yield return null;
    //         }
    //         if (isClient&&renderCam == null)
    //         {
    //             Debug.Log("못찾음");
    //             yield return null;
    //             if (GameObject.Find("RenderCamera_Client")!=null)
    //             {
    //                 Debug.Log("찾음");
    //                 renderCam = GameObject.Find("RenderCamera_Client").GetComponent<Camera>();
    //                 rt = renderCam.targetTexture;
    //                 width = rt.width;
    //                 height = rt.height;
    //                 CmdRenderTextureSize(rt.width, rt.height);
    //                 // rt.width = width;
    //                 //rt.height = height;
    //             }
    //         }
    //         if (source == null)
    //         {
    //             yield return null;
    //         }
    //         if (isClient && source != null&&renderCam!=null)
    //         {
    //             yield return endOfFrame;
    //             CompressData();
    //             
    //         }
    //     }
    // }
}