
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public class BorderlessWindow : MonoBehaviour
{
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR

    private const int GWL_STYLE = -16;

    private const long WS_OVERLAPPEDWINDOW = 0x00CF0000L;
    private const long WS_POPUP = unchecked((long)0x80000000);
    private const long WS_VISIBLE = 0x10000000L;

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_SHOWWINDOW = 0x0040;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(
        string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr64(
        IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr64(
        IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
    private static extern int GetWindowLong32(
        IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
    private static extern int SetWindowLong32(
        IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X, int Y,
        int cx, int cy,
        uint uFlags);

    private IntPtr GetGameWindow()
    {
        return FindWindow("UnityWndClass", null);
    }

    private void SetWindowStyle(IntPtr handle, long style)
    {
        IntPtr value = new IntPtr(style);

        if (IntPtr.Size == 8)
            SetWindowLongPtr64(handle, GWL_STYLE, value);
        else
            SetWindowLong32(handle, GWL_STYLE, value.ToInt32());
    }

#endif

    public IEnumerator ApplyBorderless(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);

        // Unity 창 크기 변경 대기
        yield return null;
        yield return null;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR

        IntPtr handle = GetGameWindow();

        if (handle == IntPtr.Zero)
        {
            Debug.LogError("Unity 게임 창을 찾지 못했습니다.");
            yield break;
        }

        // 제목 표시줄과 테두리 제거
        long style = WS_POPUP | WS_VISIBLE;
        SetWindowStyle(handle, style);

        // 모니터 중앙에 배치
        int x = (Display.main.systemWidth - width) / 2;
        int y = (Display.main.systemHeight - height) / 2;

        SetWindowPos(
            handle,
            IntPtr.Zero,
            x,
            y,
            width,
            height,
            SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW
        );

#endif

        Debug.Log("테두리 없는 창 모드 적용 완료");
    }

    public IEnumerator ApplyNormal(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);

        yield return null;
        yield return null;

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR

        IntPtr handle = GetGameWindow();

        if (handle == IntPtr.Zero)
        {
            Debug.LogError("Unity 게임 창을 찾지 못했습니다.");
            yield break;
        }

        // 일반 창 스타일 복구
        long style = WS_OVERLAPPEDWINDOW | WS_VISIBLE;
        SetWindowStyle(handle, style);

        int x = (Display.main.systemWidth - width) / 2;
        int y = (Display.main.systemHeight - height) / 2;

        SetWindowPos(
            handle,
            IntPtr.Zero,
            x,
            y,
            width,
            height,
            SWP_NOZORDER | SWP_FRAMECHANGED | SWP_SHOWWINDOW
        );

#endif

        Debug.Log("일반 창 모드 적용 완료");
    }
}
