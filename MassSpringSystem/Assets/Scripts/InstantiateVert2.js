#pragma strict
 /// <summary>
 
 /// Compute a skinned mesh's deformation.
 
 /// 
 
 /// The script must be attached aside a SkinnedMeshRenderer,
 
 /// which is only used to get the bone list and the mesh
 
 /// (it doesn't even need to be enabled).
 
 /// 
 
 /// Make sure the scripts accessing the results run after this one
 
 /// (otherwise you'll have a 1-frame delay),
 
 /// </summary>
 
     @HideInInspector
     var mesh: Mesh;
     @HideInInspector
     var skin: SkinnedMeshRenderer;
 
  
 @HideInInspector
    private  var vertexCount:int=0;
 @HideInInspector
      private var vertices:Vector3[];
 @HideInInspector
   var normals:Vector3[];
 
 
 public var ObjectToInstantiate : GameObject;
  
 
     function Start() {
 
         skin = GetComponent(SkinnedMeshRenderer);
 
         mesh = skin.sharedMesh;
  print(skin.name);
  
 
         vertexCount = mesh.vertexCount;
 
         vertices = new Vector3[vertexCount];//the vertices that have skin weights that need to be updated every frame (check line 115)
 
       normals = new Vector3[vertexCount];
       
        //animation example
        for (var b:int= 0; b < mesh.vertexCount; b++){
              //var cube : GameObject= new GameObject.CreatePrimitive(PrimitiveType.Cube);//the gameobject that is being instantiated
             var cube : GameObject = Instantiate(ObjectToInstantiate);
			 cube.name=b.ToString();
			 //cube.AddComponent.<Rigidbody>();
			
             cube.transform.localScale.x=0.05;
             cube.transform.localScale.y=0.05;
             cube.transform.localScale.z=0.05;
			 
 }
 print(skin.bones.Length);
     }
 
     
 
     function Update(){
  print(skin.bones.Length);
       var boneMatrices: Matrix4x4[]  = new Matrix4x4[skin.bones.Length];// this is an array of 4x4 matrices; to allow for transformations in 3D space for each of the vertices; there will be 75 bones and their resp. bone matrices
        
		 print(boneMatrices.Length);
         for (var i:int= 0; i < boneMatrices.Length; i++){
 
             boneMatrices[i] = skin.bones[i].localToWorldMatrix * mesh.bindposes[i];//read the transform from local to world space and multiply with the bind pose of each of the bones in the hierarchy
			 
          }
 
 
         for (var b:int= 0; b < mesh.vertexCount; b++){
 
               var weight:BoneWeight = mesh.boneWeights[b];//bone weights of each vertex in the Mesh
 
  //print(b);
 //Each vertex is skinned with up to four bones. All weights should sum up to one. Weights and bone indices should be defined in the order of decreasing weight. If a vertex is affected by less than four bones, the remaining weights should be zeroes 
               var bm0:Matrix4x4 = boneMatrices[weight.boneIndex0];// index of first bone
 
                var bm1:Matrix4x4 = boneMatrices[weight.boneIndex1];// index of second bone
 
                var bm2:Matrix4x4 = boneMatrices[weight.boneIndex2];// index of third bone
 
                var bm3:Matrix4x4 = boneMatrices[weight.boneIndex3];// index of fourth bone
 
  
 
                  var vertexMatrix:Matrix4x4 = new Matrix4x4();
 
  
 
             for (var n:int= 0; n < 16; n++){//each vertex in the vertexmatrix (16 elements of a 4x4 matrix) is a summation of all possible (up to 4) skinning vertex weights influencing a given bone
 
                 vertexMatrix[n] =
 
                     bm0[n] * weight.weight0 +
 
                     bm1[n] * weight.weight1 +
 
                     bm2[n] * weight.weight2 +
 
                     bm3[n] * weight.weight3;
 
             }
 
  
 
             vertices[b] = vertexMatrix.MultiplyPoint3x4(mesh.vertices[b]);
                normals[b] = vertexMatrix.MultiplyVector(mesh.normals[b]);
             
             //animation example
             var fetch= GameObject.Find( b.ToString());
             fetch.transform.position = vertices[b];
             }
 
   
             
         
 }
          