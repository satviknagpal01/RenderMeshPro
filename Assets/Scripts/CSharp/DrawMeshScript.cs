using UnityEngine;

public class DrawMeshScript : MonoBehaviour
{
    public int population;
    public float range;
    public Mesh mesh;
    public Material material;
    
    public ComputeShader compute;
    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Bounds bounds;
    private int kernel;

    private struct MeshProperties
    {
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size()
        {
            return 4 * 20;
        }
    }

    private void Setup()
    {
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        MeshProperties[] properties = new MeshProperties[population];

        kernel = compute.FindKernel("SetupMesh");
        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        compute.Dispatch(kernel, population / 64, 1, 1);
        material.SetBuffer("_Properties", meshPropertiesBuffer);
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }
}
