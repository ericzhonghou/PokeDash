Connect-ServiceFabricCluster c04dghhack7.ds.ad.adp.com:19000

Copy-ServiceFabricApplicationPackage D:\Hackathon\Final\mma\PokeDash\PokeDash\pkg\Release -ImageStoreConnectionString Fabric:ImageStore -ApplicationPackagePathInImageStore PokeDash
Register-ServiceFabricApplicationType PokeDash
New-ServiceFabricApplication fabric:/PokeDash PokeDashType 1.0.0