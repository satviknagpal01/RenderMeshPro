using UnityEngine;

public class RenderMeshScript : MonoBehaviour
{
    public int population;
    public float range;
    public Mesh mesh;
    public Material material;

    public ComputeShader compute;
    GraphicsBuffer meshPropertiesBuffer;
    GraphicsBuffer graphicsBuffer;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;

    int kernel;


    private Bounds bounds;

    RenderParams rp;

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
        graphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)population;
        graphicsBuffer.SetData(commandData);

        MeshProperties[] properties = new MeshProperties[population];

        meshPropertiesBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);

        kernel = compute.FindKernel("SetupMesh");

        compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        compute.Dispatch(kernel, population / 64, 1, 1);
        rp = new RenderParams(material);
        rp.worldBounds = bounds;
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_Properties", meshPropertiesBuffer);
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        Graphics.RenderMeshIndirect(rp, mesh, graphicsBuffer);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (graphicsBuffer != null)
        {
            graphicsBuffer.Release();
        }
        graphicsBuffer = null;
    }
}
