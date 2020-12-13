using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshInstancedIndirectDemo : MonoBehaviour { 
    public int population; 
    public float range;

    //float[] debugArrayFloat = new float[11];
    Vector3[] targetVelocityArray;
    Vector3[] cohVelocityArray;
    Vector3[] alignVelocityArray;
    Vector3[] avoidVelocityArray;

    float[] debugNeighborCountFloat;
    //Vector3[] debugCenterOfMassArray;
    //Vector3[] debugMyPositionArray;
    //Vector3[] debugCohDirArray;
    //Vector3[] debugCohTargetVelocityArray;
    // Vector3[] debugReturnCohVelocityArray;

    //Vector3[] debugCurrentDirArray;
    // float[] debugCurrentSpeedFloat;
    //Vector3[] debugTargetDirArray;
    // float[] debugTargetSpeedFloat;
    //Vector3[] debugAxisArray;
    //float[] debugSpeedRatioFloat;
    //Vector3[] debugNextDirArray;
    //float[] debugNextSpeedFloat;
    Vector3[] debugNextVelocityArray;
    float[] debugThetaFloat;

    public float deltaTime;
    public float neighborRadius;
    public float minTheta;

    [Header("Cohesion")]
    public float cohSpeed;
    public float cohWeight;

    [Header("Alignment")]
    public float alignWeight;

    [Header("Avoidance")]
    public float avoidWeight;
    public float avoidanceRadius;
    public float minDistance;

    [Header("Attract")]
    public float attractSpeed;
    public float attractWeight;
    public float distanceFromAttracter;

    Vector3 boundsSize;

    public Material material;
    public ComputeShader computeShader; 
    public Transform pusher;

    //private ComputeBuffer debugBufferFloat;
    private ComputeBuffer targetVelocityBuffer;
    private ComputeBuffer cohVelocityBuffer;
    private ComputeBuffer alignVelocityBuffer;
    private ComputeBuffer avoidVelocityBuffer;
    //private ComputeBuffer debugCenterOfMassBuffer;
    private ComputeBuffer debugNeighborCountBuffer;
    //private ComputeBuffer debugMyPositionBuffer;

    // private ComputeBuffer debugCurrentDirBuffer;
    // private ComputeBuffer debugCurrentSpeedBuffer; 
    // private ComputeBuffer debugTargetDirBuffer;
    // private ComputeBuffer debugTargetSpeedBuffer;
    //private ComputeBuffer debugAxisBuffer;
    //private ComputeBuffer debugSpeedRatioBuffer;
    //private ComputeBuffer debugNextDirBuffer;
    //private ComputeBuffer debugNextSpeedBuffer;
    private ComputeBuffer debugNextVelocityBuffer;
    //private ComputeBuffer debugCohDirBuffer;
    //private ComputeBuffer debugCohTargetVelocityBuffer;
    private ComputeBuffer debugThetaBuffer;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    public Mesh mesh; 
    private Bounds bounds;
   
    int kernel;
    MeshProperties[] properties;
    private struct MeshProperties { 
        public Matrix4x4 mat;
        public Vector4 color;
        public Vector3 velocity;

        public static int Size() {
            return
                sizeof(float) * 4 * 4 + // mat
                sizeof(float) * 4 +     // color
                sizeof(float) * 3;      // velocity 
        }
    }

    private void Setup() {
        bounds = new Bounds(this.gameObject.transform.position, Vector3.one * (range + 1));
        boundsSize = Vector3.one * (range + 1);
        InitializeBuffers(); 
    }

    private void InitializeBuffers() { 
        kernel = computeShader.FindKernel("CSMain"); 

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
        properties = new MeshProperties[population]; 
        
        for (int i = 0; i < population; i++) { 
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range/2.0f, range/2.0f), Random.Range(-range/2.0f, range/2.0f), Random.Range(-range/2.0f, range/2.0f));
            Quaternion rotation = Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = new Vector3(Random.Range(0.2f, 5.0f), Random.Range(0.2f, 5.0f), Random.Range(0.2f, 5.0f));

            props.mat = Matrix4x4.TRS(position, rotation, scale); 
            props.color = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

            props.velocity = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));

            properties[i] = props;
        }
        //for (int i = 0; i < population; i++)
        //{
        //    Debug.Log($"velocity[{i}] = {properties[i].velocity}");
        //}
        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        computeShader.SetBuffer(kernel, "_Properties", meshPropertiesBuffer); // for compute shader. 
        material.SetBuffer("_Properties", meshPropertiesBuffer); // for regular shader.

        //debugBufferFloat = new ComputeBuffer(14, 4); // float size = 4 byte
        //debugBufferFloat.SetData(debugArrayFloat);
        //computeShader.SetBuffer(kernel, "_debugBufferFloat", debugBufferFloat);

        //targetVelocityArray = new Vector3[population];
        //targetVelocityBuffer = new ComputeBuffer(population, 12);
        //targetVelocityBuffer.SetData(targetVelocityArray);
        //computeShader.SetBuffer(kernel, "_targetVelocityBuffer", targetVelocityBuffer);

        //cohVelocityArray = new Vector3[population];
        //cohVelocityBuffer = new ComputeBuffer(population, 12);
        //cohVelocityBuffer.SetData(cohVelocityArray);
        //computeShader.SetBuffer(kernel, "_cohVelocityBuffer", cohVelocityBuffer);

        //alignVelocityArray = new Vector3[population];
        //alignVelocityBuffer = new ComputeBuffer(population, 12);
        //alignVelocityBuffer.SetData(alignVelocityArray);
        //computeShader.SetBuffer(kernel, "_alignVelocityBuffer", alignVelocityBuffer);

        //avoidVelocityArray = new Vector3[population];
        //avoidVelocityBuffer = new ComputeBuffer(population, 12);
        //avoidVelocityBuffer.SetData(avoidVelocityArray);
        //computeShader.SetBuffer(kernel, "_avoidVelocityBuffer", avoidVelocityBuffer);

        //debugCenterOfMassArray = new Vector3[population];
        //debugCenterOfMassBuffer = new ComputeBuffer(population, 12);
        //debugCenterOfMassBuffer.SetData(debugCenterOfMassArray);
        //computeShader.SetBuffer(kernel, "_debugCenterOfMassBuffer", debugCenterOfMassBuffer);

        //debugNeighborCountFloat = new float[population];
        //debugNeighborCountBuffer = new ComputeBuffer(population, 4);
        //debugNeighborCountBuffer.SetData(debugNeighborCountFloat);
        //computeShader.SetBuffer(kernel, "_debugNeighborCountBuffer", debugNeighborCountBuffer);

        //debugMyPositionArray = new Vector3[population];
        //debugMyPositionBuffer = new ComputeBuffer(population, 12);
        //debugMyPositionBuffer.SetData(debugMyPositionArray);
        //computeShader.SetBuffer(kernel, "_debugMyPositionBuffer", debugMyPositionBuffer);

        //debugCurrentDirArray = new Vector3[population];
        //debugCurrentDirBuffer = new ComputeBuffer(population, 12);
        //debugCurrentDirBuffer.SetData(debugCurrentDirArray);
        //computeShader.SetBuffer(kernel, "_debugCurrentDirBuffer", debugCurrentDirBuffer);

        //debugCurrentSpeedFloat = new float[population];
        //debugCurrentSpeedBuffer = new ComputeBuffer(population, 4);
        //debugCurrentSpeedBuffer.SetData(debugCurrentSpeedFloat);
        //computeShader.SetBuffer(kernel, "_debugCurrentSpeedBuffer", debugCurrentSpeedBuffer);

        //debugTargetDirArray = new Vector3[population];
        //debugTargetDirBuffer = new ComputeBuffer(population, 12);
        //debugTargetDirBuffer.SetData(debugTargetDirArray);
        //computeShader.SetBuffer(kernel, "_debugTargetDirBuffer", debugTargetDirBuffer);

        //debugTargetSpeedFloat = new float[population];
        //debugTargetSpeedBuffer = new ComputeBuffer(population, 4);
        //debugTargetSpeedBuffer.SetData(debugCurrentSpeedFloat);
        //computeShader.SetBuffer(kernel, "_debugTargetSpeedBuffer", debugTargetSpeedBuffer);

        //debugAxisArray = new Vector3[population];
        //debugAxisBuffer = new ComputeBuffer(population, 12);
        //debugAxisBuffer.SetData(debugAxisArray);
        //computeShader.SetBuffer(kernel, "_debugAxisBuffer", debugAxisBuffer);

        //debugSpeedRatioFloat = new float[population];
        //debugSpeedRatioBuffer = new ComputeBuffer(population, 4);
        //debugSpeedRatioBuffer.SetData(debugSpeedRatioFloat);
        //computeShader.SetBuffer(kernel, "_debugSpeedRatioBuffer", debugSpeedRatioBuffer);

        //debugNextDirArray = new Vector3[population];
        //debugNextDirBuffer = new ComputeBuffer(population, 12);
        //debugNextDirBuffer.SetData(debugNextDirArray);
        //computeShader.SetBuffer(kernel, "_debugNextDirBuffer", debugNextDirBuffer);

        //debugNextSpeedFloat = new float[population];
        //debugNextSpeedBuffer = new ComputeBuffer(population, 4);
        //debugNextSpeedBuffer.SetData(debugNextSpeedFloat);
        //computeShader.SetBuffer(kernel, "_debugNextSpeedBuffer", debugNextSpeedBuffer);

        //debugNextVelocityArray = new Vector3[population];
        //debugNextVelocityBuffer = new ComputeBuffer(population, 12);
        //debugNextVelocityBuffer.SetData(debugNextVelocityArray);
        //computeShader.SetBuffer(kernel, "_debugNextVelocityBuffer", debugNextVelocityBuffer);

        //debugCohDirArray = new Vector3[population];
        //debugCohDirBuffer = new ComputeBuffer(population, 12);
        //debugCohDirBuffer.SetData(debugCohDirArray);
        //computeShader.SetBuffer(kernel, "_debugCohDirBuffer", debugCohDirBuffer);

        //debugCohTargetVelocityArray = new Vector3[population];
        //debugCohTargetVelocityBuffer = new ComputeBuffer(population, 12);
        //debugCohTargetVelocityBuffer.SetData(debugCohTargetVelocityArray);
        //computeShader.SetBuffer(kernel, "_debugCohTargetVelocityBuffer", debugCohTargetVelocityBuffer);

        //debugThetaFloat = new float[population];
        //debugThetaBuffer = new ComputeBuffer(population, 4);
        //debugThetaBuffer.SetData(debugThetaFloat);
        //computeShader.SetBuffer(kernel, "_debugThetaBuffer", debugThetaBuffer);
    }

    private void Start() {
        Setup();
    }

    private void Update() { 
        computeShader.SetVector("_PusherPosition", pusher.position); // TODO : attracter 로 변경 요망. 

        computeShader.SetFloat("_distanceFromAttracter", distanceFromAttracter);
        computeShader.SetFloat("_neighborRadius", neighborRadius);
        computeShader.SetFloat("_deltaTime", deltaTime);
        computeShader.SetInt("_population", population); 
        computeShader.SetFloat("_avoidanceRadius", avoidanceRadius);

        computeShader.SetFloat("_cohSpeed", cohSpeed);
        computeShader.SetFloat("_attractSpeed", attractSpeed);

        computeShader.SetFloat("_cohWeight", cohWeight);
        computeShader.SetFloat("_alignWeight", alignWeight);
        computeShader.SetFloat("_avoidWeight", avoidWeight);
        computeShader.SetFloat("_minDistance", minDistance);
        computeShader.SetFloat("_minTheta", minTheta);
        computeShader.SetVector("_boundSize", boundsSize);


        computeShader.Dispatch(kernel, Mathf.CeilToInt(population / 64f), 1, 1);
        // meshPropertiesBuffer.GetData(properties);
        //debugBufferFloat.GetData(debugArrayFloat);
        // targetVelocityBuffer.GetData(targetVelocityArray); // target velocity buffer 

        //cohVelocityBuffer.GetData(cohVelocityArray); // cohesion velocity buffer
        //alignVelocityBuffer.GetData(alignVelocityArray); // alignment velocity buffer
        //avoidVelocityBuffer.GetData(avoidVelocityArray); // avoidance velocity buffer

        //debugCohDirBuffer.GetData(debugCohDirArray);
        //debugCohTargetVelocityBuffer.GetData(debugCohTargetVelocityArray);

        //debugNeighborCountBuffer.GetData(debugNeighborCountFloat);
        //debugCenterOfMassBuffer.GetData(debugCenterOfMassArray);
        //debugMyPositionBuffer.GetData(debugMyPositionArray);

        //debugCurrentDirBuffer.GetData(debugCurrentDirArray);
        // debugCurrentSpeedBuffer.GetData(debugCurrentSpeedFloat);
        //debugTargetDirBuffer.GetData(debugTargetDirArray);
        // debugTargetSpeedBuffer.GetData(debugTargetSpeedFloat);
        //debugAxisBuffer.GetData(debugAxisArray);
        //debugSpeedRatioBuffer.GetData(debugSpeedRatioFloat);
        //debugNextDirBuffer.GetData(debugNextDirArray);
        //debugNextSpeedBuffer.GetData(debugNextSpeedFloat);
        //debugNextVelocityBuffer.GetData(debugNextVelocityArray);
        //debugThetaBuffer.GetData(debugThetaFloat);

        //Debug.Log($"distanceFromPusher = {distanceFromAttracter}, {debugArrayFloat[0]}");
        //Debug.Log($"neighborRadius = {neighborRadius}, {debugArrayFloat[1]}");
        //Debug.Log($"deltaTime = {deltaTime}, {debugArrayFloat[2]}");
        //Debug.Log($"avoidanceRadius = {avoidanceRadius}, {debugArrayFloat[3]}");
        //Debug.Log($"cohSpeed = {cohSpeed}, {debugArrayFloat[4]}");
        //Debug.Log($"cohWeight = {cohWeight}, {debugArrayFloat[8]}");
        //Debug.Log($"alignSpeed = {alignSpeed}, {debugArrayFloat[5]}");
        //Debug.Log($"alignWeight = {alignWeight}, {debugArrayFloat[9]}");
        //Debug.Log($"avoidSpeed = {avoidSpeed}, {debugArrayFloat[6]}");
        //Debug.Log($"avoidWeight = {avoidWeight}, {debugArrayFloat[10]}");
        //Debug.Log($"attractSpeed = {attractSpeed}, {debugArrayFloat[7]}");

        for (int i = 0; i < population; i++)
        {
            //Debug.Log($"velocity[{i}] = {properties[i].velocity}");
            //Debug.Log($"targetVelocity[{i}] = {targetVelocityArray[i]}");
            //Debug.Log($"cohVelocity[{i}] = {cohVelocityArray[i]}");
            //Debug.Log($"alignVelocity[{i}] = {alignVelocityArray[i]}");
            //Debug.Log($"avoidVelocity[{i}] = {avoidVelocityArray[i]}");
            //Debug.Log($"neighborCount[{i}] = {debugNeighborCountFloat[i]}");
            //Debug.Log($"centerOfMass[{i}] = {debugCenterOfMassArray[i]}");
            //Debug.Log($"myPosition[{i}] = {debugMyPositionArray[i]}");

            //Debug.Log($"currentDir[{i}] = {debugCurrentDirArray[i]}");
            //// Debug.Log($"currentSpeed[{i}] = {debugCurrentSpeedFloat[i]}");
            //Debug.Log($"targetDir[{i}] = {debugTargetDirArray[i]}");
            //// Debug.Log($"targetSpeed[{i}] = {debugTargetSpeedFloat[i]}");
            //Debug.Log($"axis[{i}] = {debugAxisArray[i]}");
            //Debug.Log($"speedRatio[{i}] = {debugSpeedRatioFloat[i]}");
            //Debug.Log($"nextDir[{i}] = {debugNextDirArray[i]}");
            //Debug.Log($"nextSpeed[{i}] = {debugNextSpeedFloat[i]}");
            // Debug.Log($"nextVelocity[{i}] = {debugNextVelocityArray[i]}");
            //Debug.Log($"cohDir[{i}] = {debugCohDirArray[i]}");
            //Debug.Log($"cohTargetVelocity[{i}] = {debugCohTargetVelocityArray[i]}");
            // Debug.Log($"theta[{i}] = {debugThetaFloat[i]}");
        }

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

    private void FixedUpdate()
    {
        // Debug.Log(Time.deltaTime);
    }
}