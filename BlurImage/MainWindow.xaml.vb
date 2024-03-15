Imports Emgu.CV
Imports Emgu.CV.CvEnum
Imports Emgu.CV.Structure
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

Class MainWindow

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs)
        Dim desktopBounds As Rectangle = Screen.PrimaryScreen.Bounds

        ' Capture the screenshot
        Dim screenshot As New Bitmap(desktopBounds.Width, desktopBounds.Height)
        Using g As Graphics = Graphics.FromImage(screenshot)
            g.CopyFromScreen(New Point(0, 0), New Point(0, 0), desktopBounds.Size)
        End Using

        Try
            ' Convert the screenshot to a Mat object
            Dim matScreenshot As New Mat()
            CvInvoke.CvtColor(New Image(Of Bgr, Byte)(screenshot), matScreenshot, ColorConversion.Bgr2Rgb)

            ' Apply Gaussian blur with OpenCL
            ApplyGaussianBlurOpenCL(matScreenshot, 5) ' Adjust the blur radius as needed
        Catch ex As Exception
            ' Handle and display any exceptions
            MessageBox.Show("An error occurred: " & ex.Message)
        End Try
    End Sub

    Private Sub ApplyGaussianBlurOpenCL(ByVal sourceImage As Mat, ByVal blurRadius As Double)
        Dim blurredImage As New Mat()
        CvInvoke.GaussianBlur(sourceImage, blurredImage, New Size(0, 0), blurRadius, 5)

        ' Convert Mat to Bitmap
        Dim bitmap As Bitmap = ConvertMatToBitmap(blurredImage)

        ' Set the Bitmap as the source of the image control
        imageControl.Source = ConvertBitmapToImageSource(bitmap)
    End Sub

    Private Function ConvertMatToBitmap(ByVal mat As Mat) As Bitmap
        Dim bitmap As New Bitmap(mat.Width, mat.Height, PixelFormat.Format24bppRgb)

        Dim bitmapData As BitmapData = bitmap.LockBits(New Rectangle(0, 0, mat.Width, mat.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb)
        Dim data As IntPtr = bitmapData.Scan0

        Dim matData As Byte() = New Byte(mat.Width * mat.Height * 3 - 1) {}
        Marshal.Copy(mat.DataPointer, matData, 0, matData.Length)

        For y As Integer = 0 To mat.Height - 1
            Dim offset As Integer = y * mat.Width * 3
            Marshal.Copy(matData, offset, data, mat.Width * 3)
            data += bitmapData.Stride
        Next

        bitmap.UnlockBits(bitmapData)
        Return bitmap
    End Function


    Private Function ConvertBitmapToImageSource(ByVal bitmap As Bitmap) As BitmapImage
        Dim bitmapImage As New BitmapImage()
        Using memoryStream As New MemoryStream()
            bitmap.Save(memoryStream, ImageFormat.Png)
            memoryStream.Seek(0, SeekOrigin.Begin)
            bitmapImage.BeginInit()
            bitmapImage.StreamSource = memoryStream
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad
            bitmapImage.EndInit()
        End Using
        Return bitmapImage
    End Function

    Private Declare Function BitBlt Lib "gdi32.dll" (ByVal hdcDest As IntPtr, ByVal xDest As Integer, ByVal yDest As Integer, ByVal wDest As Integer, ByVal hDest As Integer, ByVal hdcSrc As IntPtr, ByVal xSrc As Integer, ByVal ySrc As Integer, ByVal rop As Integer) As Boolean
    Private Declare Function GetDesktopWindow Lib "user32.dll" () As IntPtr

End Class
