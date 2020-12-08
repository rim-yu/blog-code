using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshInstancedIndirectDemo : MonoBehaviour { 
    public int population; 
    public float range;

    // ------------------------------
    float[] debugArrayFloat = new float[8];
    public float neighborRadius;

    [Header("Cohesion")]
    public float cohSpeed;
    [Header("Alignment")]
    public float alignSpeed;
    [Header("Avoidance")]
    public float avoidSpeed;
    public float avoidanceRadius;
    [Header("Away")]
    public float distanceFromPusher;
    public float awaySpeed;
    // ------------------------------

    public Material material;
    public ComputeShader computeShader; 
    public Transform pusher;

    // ****
    private ComputeBuffer debugBufferFloat;
    private ComputeBuffer debugBufferInt;
    // ****
    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    public Mesh mesh; 
    private Bounds bounds;

    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties { 
        public Matrix4x4 mat;
        public Vector4 color;

        // ------------------------------
        //public Vector3 cohVector;
        //public Vector3 alignVector;
        //public Vector3 avoidVector;

        //public Vector3 position;
        //public Vector3 velocity;
        // ------------------------------

        public static int Size() {
            return
                sizeof(float) * 4 * 4 +
                sizeof(float) * 4;
            //// ------------------------------
            //    sizeof(float) * 3 + // cohVector
            //    sizeof(float) * 3 + // alignVector
            //    sizeof(float) * 3 + // avoidVector
            //    sizeof(float) * 3 + // position
            //    sizeof(float) * 3;  // velocity 
            //// ------------------------------
        }
    }

    private void Setup() {
        // Mesh mesh = CreateQuad(); 
        // this.mesh = mesh;

        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        InitializeBuffers(); 
    }

    private void InitializeBuffers() { 
        int kernel = computeShader.FindKernel("CSMain");

        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)mesh.GetIndexCount(0);
        args[1] = (uint)population;
        args[2] = (uint)mesh.GetIndexStart(0);
        args[3] = (uint)mesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffer with the given population.
        MeshProperties[] properties = new MeshProperties[population];
        
        for (int i = 0; i < population; i++) { 
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = Vector3.one;

            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.color = Color.Lerp(Color.red, Color.blue, Random.value);

            // ------------------------------
            //props.cohVector = new Vector3(0, 0, 0);
            //props.alignVector = new Vector3(0, 0, 0);
            //props.avoidVector = new Vector3(0, 0, 0);

            //props.position = new Vector3(0, 0, 0);
            //props.velocity = new Vector3(0, 0, 0);
            // ------------------------------

            properties[i] = props;
        }
        
        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties); // 

        computeShader.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        material.SetBuffer("_Properties", meshPropertiesBuffer);

        // **** 
        debugBufferFloat = new ComputeBuffer(12, 4); // float size = 4 byte
        debugBufferFloat.SetData(debugArrayFloat);

        computeShader.SetBuffer(kernel, "_debugBufferFloat", debugBufferFloat);
        // ****
    }

    private void Start() {
        Setup();
    }

    private void Update() {
        int kernel = computeShader.FindKernel("CSMain");
        computeShader.SetVector("_PusherPosition", pusher.position);
        // We used to just be able to use `population` here, but it looks like a Unity update imposed a thread limit (65535) on my device.
        // This is probably for the best, but we have to do some more calculation.  Divide population by numthreads.x in the compute shader.

        // ------------------------------
        computeShader.SetFloat("_distanceFromPusher", distanceFromPusher);
        computeShader.SetFloat("_neighborRadius", neighborRadius);
        computeShader.SetFloat("_deltaTime", Time.deltaTime);
        computeShader.SetInt("_population", population);
        computeShader.SetFloat("_avoidanceRadius", avoidanceRadius);

        computeShader.SetFloat("_cohSpeed", cohSpeed);
        computeShader.SetFloat("_alignSpeed", alignSpeed);
        computeShader.SetFloat("_avoidSpeed", avoidSpeed);
        computeShader.SetFloat("_awaySpeed", awaySpeed);
        // ------------------------------

        computeShader.Dispatch(kernel, Mathf.CeilToInt(population / 64f), 1, 1); // CeilToInt : 올림

        // ****
        debugBufferFloat.GetData(debugArrayFloat);
        Debug.Log($"distanceFromPusher = {distanceFromPusher}, {debugArrayFloat[0]}");
        Debug.Log($"neighborRadius = {neighborRadius}, {debugArrayFloat[1]}");
        Debug.Log($"deltaTime = {Time.deltaTime}, {debugArrayFloat[2]}");
        Debug.Log($"avoidanceRadius = {avoidanceRadius}, {debugArrayFloat[3]}");
        Debug.Log($"cohSpeed = {cohSpeed}, {debugArrayFloat[4]}");
        Debug.Log($"alignSpeed = {alignSpeed}, {debugArrayFloat[5]}");
        Debug.Log($"avoidSpeed = {avoidSpeed}, {debugArrayFloat[6]}");
        Debug.Log($"awaySpeed = {awaySpeed}, {debugArrayFloat[7]}");
        // ****

        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable() {
        if (meshPropertiesBuffer != null) {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null) {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }
}