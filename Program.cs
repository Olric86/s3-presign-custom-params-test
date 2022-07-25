using System;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime.Internal.Auth;
using System.Web;

namespace s3_presign_custom_params_test
{

  class Program
  {
    static string bucketName = "------------";
    private static readonly RegionEndpoint bucketRegion = RegionEndpoint.EUCentral1;
    private static readonly string accessKeyId = "----------";
    private static readonly string secretAccessKey = "--------";

    static void test(GetPreSignedUrlRequest req)
    {
      var values = req.Parameters.Keys;
    }

    static void Main(string[] args)
    {
      // create a valid conf object
      AmazonS3Config config = new AmazonS3Config()
      {
        RegionEndpoint = bucketRegion,
      };
      // create an s3 client using accessKeyId and secretAccessKey
      AmazonS3Client s3Client = new AmazonS3Client(accessKeyId, secretAccessKey, config);
      
      var listObjectsReq = new ListObjectsRequest()
      {
        BucketName = bucketName,
        Marker = "test/"
      };
      // List all objects contained within bucket/test/
      // not taking into consideration possible paging here
      System.Threading.Tasks.Task<ListObjectsResponse> task = s3Client.ListObjectsAsync(listObjectsReq);
      ListObjectsResponse resp = task.Result;
      // iterate on the response
      foreach (var item in resp.S3Objects)
      {
        Console.WriteLine($"{item.BucketName}/{item.Key}");
        // Prepare a preSignedUrlRequest
        var getPreSignUrlReq = new GetPreSignedUrlRequest()
        {
          Key = item.Key,
          BucketName = bucketName,
          Expires = DateTime.UtcNow.AddSeconds(120),

        };
        string authenticationToken = "admin_|_|_|_234jkdhfoiudndkjhdsf";
        string cliSessID = "CliSessID_1234";
        string callingAE = "";
        string calledAE = "";
        Console.WriteLine("---------- Parameters ----------");
        Console.WriteLine("");
        Console.WriteLine($"AuthenticationToken:\t\t[{authenticationToken}]");
        Console.WriteLine($"CliSessID:\t\t\t[{cliSessID}]");
        Console.WriteLine($"CallingAETitle:\t\t\t[{callingAE}]");
        Console.WriteLine($"CalledAETitle:\t\t\t[{calledAE}]");
        Console.WriteLine("");
        Console.WriteLine("------------------------");


        // use ForceXKeys = false, custom modification in sdk -> ParameterCollection class
        // to allow for paramters not starting with x-
        getPreSignUrlReq.Parameters.ForceXKeys = false;
        getPreSignUrlReq.Parameters["AuthenticationToken"] = authenticationToken;
        getPreSignUrlReq.Parameters["CliSessID"] = cliSessID;
        getPreSignUrlReq.Parameters["CallingAETitle"] = callingAE;
        getPreSignUrlReq.Parameters["CalledAETitle"] = calledAE;



        var signedURL = s3Client.GetPreSignedURL(getPreSignUrlReq);

        // Try to remove the parameters from the URL
        var urlWithParams = new Uri(signedURL);
        var queryParams = HttpUtility.ParseQueryString(urlWithParams.Query);
        queryParams.Remove("AuthenticationToken");
        queryParams.Remove("CliSessID");
        queryParams.Remove("CallingAETitle");
        queryParams.Remove("CalledAETitle");

        string urlNoParams = signedURL.Replace(urlWithParams.Query, "") + "?";
        foreach (string key in queryParams.AllKeys)
        {
          urlNoParams += key + "=" + Uri.EscapeUriString(queryParams.Get(key)) + "&";
        }

        Console.WriteLine("---------- Complete signed URL ----------");
        Console.WriteLine("");
        Console.WriteLine(signedURL);
        Console.WriteLine("");
        Console.WriteLine("------------------------");

        Console.WriteLine("---------- URL with missing params ----------");
        Console.WriteLine("");
        Console.WriteLine(urlNoParams.TrimEnd('&'));
        Console.WriteLine("");
        Console.WriteLine("------------------------");
      }

    }
  }
}
