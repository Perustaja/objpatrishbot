using objpatrishbot.ImageHandler.Interface;
using Discord;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace objpatrishbot.ImageHandler.Providers.GoogleDrive
{
    class GoogleDriveHandler : IImageHandler<Attachment>
    {
        // If modifying these scopes, delete your previously saved credentials
        static string[] Scopes = { DriveService.Scope.DriveReadonly };
        static string ApplicationName = "objpatrishbot client";
        public UserCredential credential;
        string credPath = "credentials/gdrive_token.json";
        public DriveService service;
        public GoogleDriveHandler(UserCredential creds)
        {
            using (var stream = new FileStream("credentials/gdrive_credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Google Drive Credential file saved to: " + credPath);
            }

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public Attachment GetImage(string user)
        {
            FilesResource.ListRequest listRequest = service.Files.List();
            listRequest.PageSize = 100;
            listRequest.Fields = "nextPageToken, files(id, name)";

            IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute().Files.Where(o => Regex.IsMatch(o.FileExtension, @"(jpe?g|bmp|png|gif)$")).ToList();

            if (files != null && files.Count > 0)
            {
                Random rand = new Random();

                var fileId = files[rand.Next(0, files.Count-1)].Id;
                var request = service.Files.Get(fileId);
                var stream = new System.IO.MemoryStream();

                // Add a handler which will be notified on progress changes.
                // It will notify on each chunk download and when the
                // download is completed or failed.
                request.MediaDownloader.ProgressChanged +=
                (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            {
                                Console.WriteLine(progress.BytesDownloaded);
                                break;
                            }
                        case DownloadStatus.Completed:
                            {
                                return
                                break;
                            }
                        case DownloadStatus.Failed:
                            {
                                Console.WriteLine("Download failed.");
                                break;
                            }
                    }
                };
                request.Download(stream);
            }

            Attachment attachment = new Attachment();
            return attachment;
        }
    }
}