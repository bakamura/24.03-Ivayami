using UnityEngine;

public static class DebugUtilities
{
    public static Mesh CreateConeMesh(Transform transform, float visionAngle, float detectionRange)
    {
        Mesh pyramid = new Mesh();
        int numOfTriangles = 14;
        int numVertices = numOfTriangles * 3;
        float halfAngle = visionAngle / 2f;
        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        Vector3 Center = Vector3.zero;//0 

        Vector3 Left = Quaternion.Euler(0, -halfAngle, 0) * transform.forward * detectionRange;
        Vector3 Right = Quaternion.Euler(0, halfAngle, 0) * transform.forward * detectionRange;

        Vector3 top = Quaternion.Euler(-halfAngle, 0, 0) * transform.forward * detectionRange;
        Vector3 bottom = Quaternion.Euler(halfAngle, 0, 0) * transform.forward * detectionRange;

        float middlePointAngles = halfAngle * .7f;

        Vector3 MiddleTopLeft = Quaternion.Euler(-middlePointAngles, -middlePointAngles, 0) * transform.forward * detectionRange;
        Vector3 MiddleTopRight = Quaternion.Euler(-middlePointAngles, middlePointAngles, 0) * transform.forward * detectionRange;

        Vector3 MiddleBottomLeft = Quaternion.Euler(middlePointAngles, -middlePointAngles, 0) * transform.forward * detectionRange;
        Vector3 MiddleBottomRight = Quaternion.Euler(middlePointAngles, middlePointAngles, 0) * transform.forward * detectionRange;

        int currentVert = 0;

        //top left
        vertices[currentVert++] = top;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = MiddleTopLeft;

        //middle top left
        vertices[currentVert++] = MiddleTopLeft;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = Left;

        //middle bottom left
        vertices[currentVert++] = Left;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = MiddleBottomLeft;

        //bottom left
        vertices[currentVert++] = MiddleBottomLeft;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = bottom;

        //bottom right
        vertices[currentVert++] = bottom;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = MiddleBottomRight;

        //middle bottom right
        vertices[currentVert++] = MiddleBottomRight;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = Right;

        //middle top right
        vertices[currentVert++] = Right;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = MiddleTopRight;

        //top right
        vertices[currentVert++] = MiddleTopRight;
        vertices[currentVert++] = Center;
        vertices[currentVert++] = top;

        //pyramid base
        //top
        vertices[currentVert++] = top;
        vertices[currentVert++] = MiddleTopLeft;
        vertices[currentVert++] = MiddleTopRight;

        //top middle
        vertices[currentVert++] = MiddleTopRight;
        vertices[currentVert++] = MiddleTopLeft;
        vertices[currentVert++] = Right;

        vertices[currentVert++] = Right;
        vertices[currentVert++] = MiddleTopLeft;
        vertices[currentVert++] = Left;

        //bottom middle
        vertices[currentVert++] = Left;
        vertices[currentVert++] = MiddleBottomRight;
        vertices[currentVert++] = Right;

        vertices[currentVert++] = MiddleBottomRight;
        vertices[currentVert++] = Left;
        vertices[currentVert++] = MiddleBottomLeft;

        //bottom
        vertices[currentVert++] = MiddleBottomLeft;
        vertices[currentVert++] = bottom;
        vertices[currentVert++] = MiddleBottomRight;

        for (int i = 0; i < numVertices; i++) triangles[i] = i;
        pyramid.vertices = vertices;
        pyramid.triangles = triangles;
        pyramid.RecalculateNormals();

        return pyramid;
    }
}
