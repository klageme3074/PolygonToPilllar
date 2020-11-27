using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KBMEditor
{
    class MathUtil
    {
        //점 p가 v1 v2 v3 삼각형 안에 있는지 판별
        public static bool IsPointInTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 p)
        {
            return IsConvex(v1, v2, p) && IsConvex(v2, v3, p) && IsConvex(v3, v1, p);
        }

        public static bool IsPointInTriangle(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 p)
        {
            bool isWithinTriangle = false;

            //Based on Barycentric coordinates
            float denominator = ((v2.y - v3.y) * (v1.x - v3.x) + (v3.x - v2.x) * (v1.y - v3.y));

            float a = ((v2.y - v3.y) * (p.x - v3.x) + (v3.x - v2.x) * (p.y - v3.y)) / denominator;
            float b = ((v3.y - v1.y) * (p.x - v3.x) + (v1.x - v3.x) * (p.y - v3.y)) / denominator;
            float c = 1 - a - b;

            if (a > 0f && a < 1f && b > 0f && b < 1f && c > 0f && c < 1f)
            {
                isWithinTriangle = true;
            }

            return isWithinTriangle;
        }

        //교차 점 찾기
        public static bool IntersectionPoint(Vector2 AP1, Vector2 AP2, Vector2 BP1, Vector2 BP2, out Vector2 point)
        {
            point = Vector2.zero;
            
            float t;
            float s;

            float under = (BP2.y - BP1.y) * (AP2.x - AP1.x) - (BP2.x - BP1.x) * (AP2.y - AP1.y);
            if (under == 0)
                return false;

            float _t = (BP2.x - BP1.x) * (AP1.y - BP1.y) - (BP2.y - BP1.y) * (AP1.x - BP1.x);
            float _s = (AP2.x - AP1.x) * (AP1.y - BP1.y) - (AP2.y - AP1.y) * (AP1.x - BP1.x);

            t = _t / under;
            s = _s / under;

            if (t < 0.0 || t > 1.0 || s < 0.0 || s > 1.0) return false;
            if (_t == 0 && _s == 0) return false;

            point.x = AP1.x + t * (AP2.x - AP1.x);
            point.y = AP1.y + t * (AP2.y - AP1.y);

            return true;
        }

        //교차 점 찾기
        public static bool IntersectionPoint(Vector3 a, Vector3 b, Vector3 c, Vector3 d, out Vector3 point)
        {
            float det = ScalarCross(b - a, d - c);
            if (Mathf.Abs(det) < 0.0001f)
            {
                point = new Vector3(-1, -1, -1);
                return false;
            }
            point = a + (b - a) * (ScalarCross(c - a, d - c) / det);
            return true;
        }
        
        static float ScalarCross(Vector3 a, Vector3 b)
        {
            return a.x * b.z - a.z * b.x;
        }

        //예각인지 확인
        public static bool IsConvex(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            Vector3 V1 = Vector3ToVector2_XZ(v1);
            Vector3 V2 = Vector3ToVector2_XZ(v2);
            Vector3 V3 = Vector3ToVector2_XZ(v3);
            Vector3 cross = Vector3.Cross(V1 - V2, Vector3.up);
            return Vector3.Dot(cross, V3 - V2) >= 0 ? true : false;
        }
        
        //시계방향인지 확인
        public static bool IsTriangleOrientedClockwise(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 cross = Vector3.Cross(v1 - v2, v3 - v2);
            return Vector3.Dot(cross,Vector3.up) >= 0 ? true : false;
        }

        //시계방향인지 확인
        public static bool IsTriangleOrientedClockwise(Vector2 v1, Vector2 v2, Vector2 v3)
        {
            bool isClockWise = true;

            float determinant = v1.x * v2.y + v3.x * v1.y + v2.x * v3.y - v1.x * v3.y - v3.x * v2.y - v2.x * v1.y;

            if (determinant > 0f)
            {
                isClockWise = false;
            }

            return isClockWise;
        }

        
        public static Vector3 Vector3ToVector2_XZ(Vector2 inVec)
        {
            return new Vector3(inVec.x, 0f, inVec.y);
        }
        
        public static int ListClamp(int index,int listSize)
        {
            if (index >= 0)
            {
                return index % listSize;
            }
            else
            {
                return (index + listSize) % listSize;
            }
        }
    }
}