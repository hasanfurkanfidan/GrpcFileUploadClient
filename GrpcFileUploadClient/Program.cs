using Google.Protobuf;
using Grpc.Net.Client;
using GrpcFileService;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcFileUploadClient
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new FileServer.FileServerClient(channel);
            //File Upload
            string file = @"C:\Users\Furkan\Desktop\c6bdb32b-0e8b-4967-991f-f6454ff711ff.jpg";
            byte[] buffer = new byte[2048];
            using FileStream fileStream = new FileStream(file,FileMode.Open);
            var content = new AllFile
            {
                FileSize = fileStream.Length,
                Info = new GrpcFileService.FileInfo
                {
                    FileName = Path.GetFileNameWithoutExtension(fileStream.Name),
                    FileExtention = Path.GetExtension(fileStream.Name),

                },
                ReadedByte = 0
            };
            var result = client.FileUpload();
            while ((content.ReadedByte = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                content.Buffers = ByteString.CopyFrom(buffer);
                await result.RequestStream.WriteAsync(content);

            }
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            while (await result.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                Console.WriteLine($"{result.ResponseStream.Current.Message} - %{result.ResponseStream.Current.Percent}");
            }
        }
    }
}
