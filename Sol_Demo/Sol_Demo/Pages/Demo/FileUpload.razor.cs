using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Components.Forms;

namespace Sol_Demo.Pages.Demo
{
    public partial class FileUpload
    {
        #region Public Property

        [Inject]
        public IWebHostEnvironment WebHostEnvironment { get; set; }

        [Parameter]
        public int MaxAllowedFiles { get; set; }

        #endregion Public Property

        #region Private Property

        private String UploadMessage { get; set; } = "No file(s) selected";

        private IReadOnlyList<IBrowserFile> SelectedFiles { get; set; }

        private IList<String> ImageDataUrls { get; set; } = new List<String>();

        #endregion Private Property

        #region Private Method

        private async Task UploadFileToWebRoot(IBrowserFile files)
        {
            Stream stream = files.OpenReadStream();
            var path = $"{WebHostEnvironment.WebRootPath}\\files\\{files.Name}";

            FileStream fileStream = File.Create(path);
            await stream.CopyToAsync(fileStream);

            stream.Close();
            fileStream.Close();
        }

        private async Task DisplayImages(IBrowserFile imageFile)
        {
            var format = "image/png";

            var resizedImageFile = await imageFile.RequestImageFileAsync(format, 100, 100);
            var buffer = new byte[resizedImageFile.Size];
            await resizedImageFile.OpenReadStream().ReadAsync(buffer);
            var imageDataUrl = $"data:{format};base64,{Convert.ToBase64String(buffer)}";

            ImageDataUrls.Add(imageDataUrl);
            await Task.Delay(1000);
            this.StateHasChanged();
        }

        #endregion Private Method

        #region Ui Event

        private Task OnInputFileChange(InputFileChangeEventArgs e)
        {
            return Task.Run(() =>
            {
                base.InvokeAsync(() =>
                {
                    try
                    {
                        SelectedFiles = e.GetMultipleFiles(maximumFileCount: MaxAllowedFiles);
                        UploadMessage = $"{SelectedFiles.Count} files(s) selected";
                    }
                    catch (Exception ex)
                    {
                        UploadMessage = ex.Message;
                    }
                    this.StateHasChanged();
                });
            });
        }

        private async Task OnSubmit()
        {
            if (this.SelectedFiles != null && this.SelectedFiles?.Count >= 1)
            {
                foreach (var files in this.SelectedFiles)
                {
                    await this.UploadFileToWebRoot(files);
                    await this.DisplayImages(files);
                }

                UploadMessage = $"{this.SelectedFiles.Count} file(s) uploaded on server";
                this.StateHasChanged();
            }
        }

        #endregion Ui Event
    }
}