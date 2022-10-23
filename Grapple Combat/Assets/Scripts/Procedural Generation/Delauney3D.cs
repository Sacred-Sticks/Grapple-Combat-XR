using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Delauney3D : MonoBehaviour
{
    #region Classes

    public class Tetrahedron
    {
        public Node a;
        public Node b;
        public Node c;
        public Node d;

        public Node origin;
        public float radius;

        public bool isBad;

        public Tetrahedron()
        {

        }

        public Tetrahedron(Node a, Node b, Node c, Node d)
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
                a.Position.x, a.Position.y, a.Position.z,
                b.Position.x, b.Position.y, b.Position.z,
                c.Position.x, c.Position.y, c.Position.z,
                d.Position.x, d.Position.y, d.Position.z);

            radius = Vector3.Distance(origin.Position, a.Position);
            
        }

        public Node GetCircumsphereOrigin(
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

            return new(circumcenter);
        }

        public bool ContainsVertex(Node v)
        {
            return (v == this.a || v == this.b || v == this.c || v == this.d);
        }
    }

    public class Triangle
    {
        public Node u;
        public Node v;
        public Node w;

        public bool isBad;

        public Triangle()
        {

        }

        public Triangle(Node u, Node v, Node w)
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
    public List<Node> Nodes { get; private set; } = new();
    public List<Branch> Edges { get; private set; } = new();
    public List<Triangle> Triangles { get; private set; } = new();
    public List<Tetrahedron> Tetrahedra { get; private set; } = new();
    #endregion

    public static bool AlmostEqual(Node left, Node right)
    {
        return (left.Position - right.Position).sqrMagnitude < 0.01f;
    }

    public void BowyerWatson(List<Node> Nodes)
    {
        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        
        foreach (Node node in Nodes) // Get min and max for each position coordinate
        {
            if (node.Position.x < minX) minX = node.Position.x;
            if (node.Position.x > maxX) maxX = node.Position.x;
            if (node.Position.y < minY) minY = node.Position.y;
            if (node.Position.y > maxY) maxY = node.Position.y;
            if (node.Position.z < minZ) minZ = node.Position.z;
            if (node.Position.z > maxZ) maxZ = node.Position.z;
        }

        float dx = maxX - minX;
        float dy = maxY - minY;
        float dz = maxZ - minZ;
        float maxDelta = Mathf.Max(dx, dy, dz) * 5;
        float offset = 50;
        Node p1 = new(new Vector3(minX - offset, minY - offset, minZ - offset));
        Node p2 = new(new Vector3(maxX + maxDelta, minY - offset, minZ - offset));
        Node p3 = new(new Vector3(minX - offset, maxY + maxDelta, minZ - offset));
        Node p4 = new(new Vector3(minX - offset, minY - offset, maxZ + maxDelta));

        Tetrahedra.Add(new Tetrahedron(p1, p2, p3, p4));

        foreach (Node node in Nodes)
        {
            List<Triangle> triangles = new();
            foreach (var t in Tetrahedra)
            {
                if (t.radius > Vector3.Distance(t.origin.Position, node.Position))
                {
                    t.isBad = true;
                    triangles.Add(new Triangle(t.a, t.b, t.c));
                    triangles.Add(new Triangle(t.a, t.b, t.d));
                    triangles.Add(new Triangle(t.a, t.c, t.d));
                    triangles.Add(new Triangle(t.b, t.c, t.d));
                }
            }

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
                            break;
                        }
                    }
                }
                
            }

            Tetrahedra.RemoveAll((Tetrahedron t) => t.isBad);
            triangles.RemoveAll((Triangle t) => t.isBad);

            foreach (Triangle triangle in triangles)
            {
                Tetrahedra.Add(new Tetrahedron(triangle.u, triangle.v, triangle.w, node));
            }
        }

        Tetrahedra.RemoveAll((Tetrahedron t) => t.ContainsVertex(p1) || t.ContainsVertex(p2) || t.ContainsVertex(p3) || t.ContainsVertex(p4));

        HashSet<Triangle> triangleHash = new();
        HashSet<Branch> edgeHash = new();

        foreach (var t in Tetrahedra)
        {
            Node a = t.a;
            Node b = t.b;
            Node c = t.c;
            Node d = t.d;

            var abc = new Triangle(a, b, c);
            var abd = new Triangle(a, b, d);
            var acd = new Triangle(a, c, d);
            var bcd = new Triangle(b, c, d);

            if (triangleHash.Add(abc)) Triangles.Add(abc);

            if (triangleHash.Add(abd)) Triangles.Add(abd);

            if (triangleHash.Add(acd)) Triangles.Add(acd);
            
            if (triangleHash.Add(bcd)) Triangles.Add(bcd);


            var ab = new Branch(a, b);
            var ac = new Branch(a, c);
            var ad = new Branch(a, d);
            var bc = new Branch(b, c);
            var bd = new Branch(b, d);
            var cd = new Branch(c, d);

            if (edgeHash.Add(ab)) Edges.Add(ab);

            if (edgeHash.Add(ac)) Edges.Add(ac);
 
            if (edgeHash.Add(ad)) Edges.Add(ad);

            if (edgeHash.Add(bc)) Edges.Add(bc);

            if (edgeHash.Add(bd)) Edges.Add(bd);
            
            if (edgeHash.Add(cd)) Edges.Add(cd);
        }
    }
}
