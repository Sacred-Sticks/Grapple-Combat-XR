using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Edge
{
    public Vertex u;
    public Vertex v;

    public Edge()
    {

    }

    public Edge(Vertex u, Vertex v)
    {
        this.u = u;
        this.v = v;
    }

    public static bool CompareEdges(Edge left, Edge right)
    {
        return (left.u == right.u || left.u == right.v) && (left.v == right.u || left.v == right.v);
    }
}

public class Delauney3D : MonoBehaviour
{
    #region Classes

    public class Tetrahedron
    {
        public Vertex a;
        public Vertex b;
        public Vertex c;
        public Vertex d;

        public Vertex origin;
        public float radius;

        public bool isBad;

        public Tetrahedron()
        {

        }

        public Tetrahedron(Vertex a, Vertex b, Vertex c, Vertex d)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.d = d;

            GetStats();
        }

        public void GetStats()
        {
            origin = GetCircumsphereOrigin(
                a.position.x, a.position.y, a.position.z,
                b.position.x, b.position.y, b.position.z,
                c.position.x, c.position.y, c.position.z,
                d.position.x, d.position.y, d.position.z);

            radius = Vector3.Distance(origin.position, a.position);
            
        }

        public Vertex GetCircumsphereOrigin(
        double ax, double ay, double az,
        double bx, double by, double bz,
        double cx, double cy, double cz,
        double dx, double dy, double dz)
        {
            double denominator;

            double bax = bx - ax;
            double bay = by - ay;
            double baz = bz - az;
            double cax = cx - ax;
            double cay = cy - ay;
            double caz = cz - az;
            double dax = dx - ax;
            double day = dy - ay;
            double daz = dz - az;


            // Square the lengths of each edge
            double lenBA = bax * bax + bay * bay + baz * baz;
            double lenCA = cax * cax + cay * cay + caz * caz;
            double lenDA = dax * dax + day * day + daz * daz;

            // Cross the products of the edges

            // B cross C

            double crossBCX = (bay * caz - cay * baz);
            double crossBCY = (baz * cax - caz * bax);
            double crossBCZ = (bax * cay - cax * bay);

            // C cross D

            double crossCDX = (cay * daz - day * caz);
            double crossCDY = (caz * dax - daz * cax);
            double crossCDZ = (cax * day - dax * cay);

            // D cross B

            double crossDBX = (day * baz - bay * daz);
            double crossDBY = (daz * bax - baz * dax);
            double crossDBZ = (dax * bay - bax * day);

            denominator = 0.5f / (bax * crossCDX + bay * crossCDY + baz * crossCDZ);

            // Calculate offset of circumcenter from a
            Vector3 circumcenter = new
                ((float)((lenBA * crossCDX + lenCA * crossDBX + lenDA * crossBCX) * denominator + ax),
                (float)((lenBA * crossCDY + lenCA * crossDBY + lenDA * crossBCY) * denominator + ay),
                (float)((lenBA * crossCDZ + lenCA * crossDBZ + lenDA * crossBCZ) * denominator + az));
            Vertex v = new();
            v.position = circumcenter;
            return v;
        }

        public bool ContainsVertex(Vertex v)
        {
            return (v == this.a || v == this.b || v == this.c || v == this.d);
        }
    }

    public class Triangle
    {
        public Vertex u;
        public Vertex v;
        public Vertex w;

        public bool isBad;

        public Triangle()
        {

        }

        public Triangle(Vertex u, Vertex v, Vertex w)
        {
            this.u = u;
            this.v = v;
            this.w = w;
        }

        public static bool AlmostEqual(Triangle left, Triangle right)
        {
            return (
                ((Delauney3D.AlmostEqual(left.u, right.u)) || (Delauney3D.AlmostEqual(left.u, right.v)) || (Delauney3D.AlmostEqual(left.u, right.w))) &&
                ((Delauney3D.AlmostEqual(left.v, right.u)) || (Delauney3D.AlmostEqual(left.v, right.v)) || (Delauney3D.AlmostEqual(left.v, right.w))) &&
                ((Delauney3D.AlmostEqual(left.w, right.u)) || (Delauney3D.AlmostEqual(left.w, right.v)) || (Delauney3D.AlmostEqual(left.w, right.w))));
        }
    }

    #endregion

    #region Data
    public List<Vertex> Vertices { get; private set; } = new();
    public List<Edge> Edges { get; private set; } = new();
    public List<Triangle> Triangles { get; private set; } = new();
    public List<Tetrahedron> Tetrahedra { get; private set; } = new();
    #endregion

    public static bool AlmostEqual(Vertex left, Vertex right)
    {
        return (left.position - right.position).sqrMagnitude < 0.01f;
    }

    public void BowyerWatson(List<Vertex> vertices)
    {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        
        foreach (Vertex vertex in vertices) // Get min and max for each position coordinate
        {
            if (vertex.position.x < minX) minX = vertex.position.x;
            if (vertex.position.x > maxX) maxX = vertex.position.x;
            if (vertex.position.y < minY) minY = vertex.position.y;
            if (vertex.position.y > maxY) maxY = vertex.position.y;
            if (vertex.position.z < minZ) minZ = vertex.position.z;
            if (vertex.position.z > maxZ) maxZ = vertex.position.z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float maxDelta = Mathf.Max(dx, dy, dz) * 5;
        float offset = 50;
        Vertex p1 = new Vertex();
        Vertex p2 = new Vertex();
        Vertex p3 = new Vertex();
        Vertex p4 = new Vertex();
        p1.position = new Vector3(minX - offset  , minY - offset  , minZ - offset  );
        p2.position = new Vector3(maxX + maxDelta, minY - offset  , minZ - offset  );
        p3.position = new Vector3(minX - offset  , maxY + maxDelta, minZ - offset  );
        p4.position = new Vector3(minX - offset  , minY - offset  , maxZ + maxDelta);

        Tetrahedra.Add(new Tetrahedron(p1, p2, p3, p4));

        foreach (Vertex vertex in vertices)
        {
            List<Triangle> triangles = new();
            foreach (var t in Tetrahedra)
            {
                if (t.radius > Vector3.Distance(t.origin.position, vertex.position))
                {
                    t.isBad = true;
                    triangles.Add(new Triangle(t.a, t.b, t.c));
                    triangles.Add(new Triangle(t.a, t.b, t.d));
                    triangles.Add(new Triangle(t.a, t.c, t.d));
                    triangles.Add(new Triangle(t.b, t.c, t.d));
                }
            }

            //for (int i = 0; i < triangles.Count; i++)
            //{
            //    for (int j = 0; j < triangles.Count; j++)
            //    {
            //        if (Triangle.CompareTriangles(triangles[i], triangles[j]))
            //        {
            //            triangles[i].isBad = true;
            //            triangles[j].isBad = true;
            //        }
            //    }
            //}

            foreach (var t in triangles)
            {
                int count = 0;
                for (int i = 0; i < triangles.Count; i++)
                {
                    if (Triangle.AlmostEqual(t, triangles[i]))
                    {
                        count++;
                        if (count > 1)
                        {
                            t.isBad = true;
                        }
                    }
                }
                
            }

            Tetrahedra.RemoveAll((Tetrahedron t) => t.isBad);
            triangles.RemoveAll((Triangle t) => t.isBad);

            foreach (Triangle triangle in triangles)
            {
                Tetrahedra.Add(new Tetrahedron(triangle.u, triangle.v, triangle.w, vertex));
            }
        }

        Tetrahedra.RemoveAll((Tetrahedron t) => t.ContainsVertex(p1) || t.ContainsVertex(p2) || t.ContainsVertex(p3) || t.ContainsVertex(p4));

        HashSet<Triangle> triangleHash = new();
        HashSet<Edge> edgeHash = new();

        foreach (var t in Tetrahedra)
        {
            Vertex a = t.a;
            Vertex b = t.b;
            Vertex c = t.c;
            Vertex d = t.d;

            var abc = new Triangle(a, b, c);
            var abd = new Triangle(a, b, d);
            var acd = new Triangle(a, c, d);
            var bcd = new Triangle(b, c, d);

            if (triangleHash.Add(abc)) Triangles.Add(abc);

            if (triangleHash.Add(abd)) Triangles.Add(abd);

            if (triangleHash.Add(acd)) Triangles.Add(acd);
            
            if (triangleHash.Add(bcd)) Triangles.Add(bcd);


            var ab = new Edge(a, b);
            var ac = new Edge(a, c);
            var ad = new Edge(a, d);
            var bc = new Edge(b, c);
            var bd = new Edge(b, d);
            var cd = new Edge(c, d);

            if (edgeHash.Add(ab)) Edges.Add(ab);

            if (edgeHash.Add(ac)) Edges.Add(ac);
 
            if (edgeHash.Add(ad)) Edges.Add(ad);

            if (edgeHash.Add(bc)) Edges.Add(bc);

            if (edgeHash.Add(bd)) Edges.Add(bd);
            
            if (edgeHash.Add(cd)) Edges.Add(cd);
        }

    }

    private void OnDrawGizmos()
    {
        if (Edges == null) return;

        Gizmos.color = Color.blue;

        foreach (Edge edge in Edges)
        {
            Gizmos.DrawLine(edge.u.position, edge.v.position);
        }
    }
}
