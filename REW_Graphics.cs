using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;

namespace REWL
{
    public class REWGraphics
    {
        public static int FacesToEdges(List<Face> faces, out List<Edge> edgesOutput)
        {
            List<Edge> edges = new List<Edge>();
            foreach (Face face in faces)
            {
                List<Vertex> vertices = face.vertices.ToList();
                for (int i = 0; i < vertices.Count - 1; i++)
                {
                    for (int j = i + 1; j < vertices.Count; j++)
                    {
                        edges.Add(new Edge(vertices[i], vertices[j]));
                    }
                }
            }
            List<int> removeList = new List<int>();
            //foreach(Edge edge in edges)
            //{
            //    if(edges.FindAll(x => (x.v1 == edge.v1 && x.v2 == edge.v2)).Count > 1)
            //    {
            //        edges.Remove(edges.FindLast(x => (x.v1 == edge.v1 && x.v2 == edge.v2)));
            //    }
            //    if (edges.FindAll(x => (x.v1 == edge.v2 && x.v2 == edge.v1)).Count > 1)
            //    {
            //        edges.Remove(edges.FindLast(x => (x.v1 == edge.v2 && x.v2 == edge.v1)));
            //    }
            //}
            edgesOutput = edges;
            return edges.Count;
        }
    }

    public class Camera
    {
        public Vector3 e;//position  （translate To 0,0,0）
        public Vector3 g;//forward  （rotate To -z）
        public Vector3 t;//up  （rotate To +y）
        public Vector3 gxt { get { return Vector3.Cross(g, t); } } //right  （rotate To +x）

        public float near; // near = 1 / tan(fov / 2)
        public float far;
        public float aspectRatio;
        public float FOV { get { return Convert.ToSingle(Math.Atan(1d / near) * 2d); } }
    }

    public class Vertex
    {
        public Vector3 position;

        public Vertex(float x, float y, float z)
        {
            position = new Vector3(x, y, z);
        }
    }

    public class Edge
    {
        public Vertex v1;
        public Vertex v2;

        public Edge(Vertex vertex1, Vertex vertex2)
        {
            this.v1 = vertex1;
            this.v2 = vertex2;
        }
    }

    public class Face
    {
        public Vertex[] vertices;

        public Face(List<Vertex> _vertices)
        {
            vertices = _vertices.ToArray();
        }

    }

    public class Mesh
    {
        public Vertex[] vertices;
        public Edge[] edges;
        public Face[] faces;

        public Mesh(int numOfVertices, int numOfEdges, int numOfFaces)
        {
            vertices = new Vertex[numOfVertices];
            edges = new Edge[numOfEdges];
            faces = new Face[numOfFaces];
        }

        public void Translate(float x, float y, float z)
        {
            foreach (Vertex v in vertices)
            {
                v.position.x += x;
                v.position.y += y;
                v.position.z += z;
            }
        }

        /// <summary>
        /// 读取OBJ文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public List<Mesh> FromFile(string fileName)
        {
            List<Mesh> meshObjs = new List<Mesh>();

            if (File.Exists(fileName))
            {
                StreamReader streamRead = new StreamReader(fileName);
                List<Vertex> vertexList = new List<Vertex>();
                List<Face> faceList = new List<Face>();

                while (!streamRead.EndOfStream)
                {
                    string str = streamRead.ReadLine();
                    while (str.ToCharArray()[0] == 'o')
                    {
                        int verticesNumOfObject = 0;
                        int edgeNumOfObject = 0;
                        int facesNumOfObject = 0;
                        List<Vertex> vertexListOfObject = new List<Vertex>();
                        List<Edge> edgeListOfObject = new List<Edge>();
                        List<Face> faceListOfObject = new List<Face>();

                        str = streamRead.ReadLine();
                        while (str.Substring(0, 2) == "v ")
                        {
                            verticesNumOfObject += 1;
                            string strPosition = str.Substring(2);
                            Match matchX = Regex.Matches(strPosition, "-?[0-9]+\\.[0-9]+")[0];
                            Match matchY = Regex.Matches(strPosition, "-?[0-9]+\\.[0-9]+")[1];
                            Match matchZ = Regex.Matches(strPosition, "-?[0-9]+\\.[0-9]+")[2];
                            float x; Single.TryParse(matchX.Value, out x);
                            float y; Single.TryParse(matchY.Value, out y);
                            float z; Single.TryParse(matchZ.Value, out z);

                            vertexList.Add(new Vertex(x, y, z));
                            vertexListOfObject.Add(new Vertex(x, y, z));

                            str = streamRead.ReadLine();
                        }
                        while (str.Substring(0, 2) == "vt")
                        {
                            str = streamRead.ReadLine();
                        }
                        while (str.Substring(0, 2) == "vn")
                        {
                            str = streamRead.ReadLine();
                        }
                        while (str.Substring(0, 2) == "us")
                        {
                            str = streamRead.ReadLine();
                        }
                        while (str.Substring(0, 2) == "s ")
                        {
                            str = streamRead.ReadLine();
                        }
                        while (str.Substring(0, 2) == "f " || str.Substring(0, 2) == "us")
                        {
                            if (str.Substring(0, 2) == "f ")
                            {
                                //face
                                facesNumOfObject += 1;
                                string strPosition = str.Substring(2);
                                MatchCollection matches = Regex.Matches(strPosition, "[0-9]+/[0-9]*/[0-9]*");
                                List<Vertex> verticesOfCurrentFace = new List<Vertex>();
                                foreach (Match match in matches)
                                {
                                    int vertexIndex; int.TryParse(match.Value.Substring(0, match.Value.IndexOf("/")), out vertexIndex);
                                    //MessageBox.Show(vertexIndex.ToString() + "  listcount: " + vertexList.Count.ToString());
                                    verticesOfCurrentFace.Add(vertexList[vertexIndex - 1]);
                                }
                                faceList.Add(new Face(verticesOfCurrentFace));
                                faceListOfObject.Add(new Face(verticesOfCurrentFace));
                            }
                            else
                            {
                                //usemtl
                            }

                            str = streamRead.ReadLine();
                            if (str == null)
                            {
                                break;
                            }
                        }
                        edgeNumOfObject = REWGraphics.FacesToEdges(faceList, out edgeListOfObject);
                        //MessageBox.Show(faceList.Count.ToString() + "==>" + edgeNum.ToString());
                        Mesh meshAdd = new Mesh(verticesNumOfObject, edgeNumOfObject, facesNumOfObject);
                        meshAdd.vertices = vertexListOfObject.ToArray();
                        meshAdd.edges = edgeListOfObject.ToArray();
                        meshObjs.Add(meshAdd);
                        if (str == null)
                        {
                            break;
                        }
                    }
                }
                streamRead.Close();
            }
            return meshObjs;
        }

        /// <summary>
        /// 使用GDI+绘制
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="size"></param>
        /// <param name="graphics"></param>
        /// <param name="linecolor"></param>
        public void DrawCall(Camera cam, Size size, Graphics graphics, Color linecolor = new Color())
        {
            //V视图变换矩阵
            Matrix4x4 mTview = new Matrix4x4(
                1, 0, 0, -cam.e.x,
                0, 1, 0, -cam.e.y,
                0, 0, 1, -cam.e.z,
                0, 0, 0, 1
                );
            Matrix4x4 mRview = new Matrix4x4(
                cam.gxt.x, cam.gxt.y, cam.gxt.z, 0,
                cam.t.x, cam.t.y, cam.t.z, 0,
                -cam.g.x, -cam.g.y, -cam.g.z, 0, //g方向朝向-z方向
                0, 0, 0, 1
                );
            Matrix4x4 mV = mRview * mTview;
            //P投影变换矩阵
            Matrix4x4 mPerspToOrtho = new Matrix4x4(
                cam.near / cam.aspectRatio, 0, 0, 0,
                0, cam.near, 0, 0,
                0, 0, cam.near + cam.far, -cam.near * cam.far,
                0, 0, 1, 0
                );
            Matrix4x4 mTprojection = new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
            Matrix4x4 mSprojection = new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
                );
            Matrix4x4 mP = mSprojection * mTprojection * mPerspToOrtho;

            //顶点处理
            foreach (Vertex vertex in vertices)
            {
                Vector3 finalPosition = (mP * mV * vertex.position.ToVector4(1f)).ToVector3();
            }
            //边处理
            foreach (Edge edge in edges)
            {
                Vector3 finalPosV1 = (mP * mV * edge.v1.position.ToVector4(1f)).ToVector3();
                Vector3 finalPosV2 = (mP * mV * edge.v2.position.ToVector4(1f)).ToVector3();

                Point pointV1 = new Point(finalPosV1.ToPanelPoint(size).X, finalPosV1.ToPanelPoint(size).Y);
                Point pointV2 = new Point(finalPosV2.ToPanelPoint(size).X, finalPosV2.ToPanelPoint(size).Y);

                if (Vector3.Dot((edge.v1.position + edge.v2.position) * 0.5f - cam.e, cam.g) > 0)
                {
                    float depth1 = finalPosV1.z;
                    float depth2 = finalPosV2.z;
                    int intDepth1;
                    int intDepth2;

                    if (depth1 < cam.far) //远平面
                    {
                        intDepth1 = -255;
                    }
                    else if (depth1 > cam.near) //近平面
                    {
                        intDepth1 = 0;
                    }
                    else
                    {
                        intDepth1 = Convert.ToInt32(depth1 * (-255f / cam.far));
                    }
                    if (depth2 < cam.far) //远平面
                    {
                        intDepth2 = -255;
                    }
                    else if (depth2 > cam.near) //近平面
                    {
                        intDepth2 = 0;
                    }
                    else
                    {
                        intDepth2 = Convert.ToInt32(depth2 * (-255f / cam.far));
                    }

                    if (((depth1 + depth2) / 2) < cam.near && ((depth1 + depth2) / 2) > cam.far)
                    {
                        if (linecolor != Color.Empty)
                        {
                            graphics.DrawLine(new Pen(linecolor), pointV1, pointV2);
                        }
                        else
                        {
                            graphics.DrawLine(new Pen(Color.White), pointV1, pointV2);
                        }

                    }
                }

            }
        }
    }


    



    public static class PositionToScreenPoint
    {
        public static Point ToPanelPoint(this Vector3 position, Size size) //position:[-1, 1]^2
        {
            int width = size.Width;
            int height = size.Height;

            //viewport视口变换矩阵
            Matrix4x4 mViewport = new Matrix4x4(
                (float)width / 2f, 0, 0, (float)width / 2f,
                0, -((float)height / 2f), 0, (float)height / 2f,
                0, 0, 1f, 0,
                0, 0, 0, 1f
                );
            Vector3 screenPos = (mViewport * position.ToVector4(1f)).ToVector3();
            int x = Convert.ToInt32(Math.Round(screenPos.x));
            int y = Convert.ToInt32(Math.Round(screenPos.y));
            return new Point(x, y);
        }
    }
}
