using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnitTestTable : MonoBehaviour
{
    public Grid grid_; // Tilemap的Grid组件
    public AsGrid asGrid;

    public bool bool_findpathmove_chick = false;
    public TestUnit basicUnit_test;

    public bool bool_findpathmove_chick2 = false;
    public TestUnit basicUnit_test2;

    public bool bool_drawpoint_chick = false;
    public Color color_debug_chickpoint_celltoworld = Color.yellow;
    public Color color_debug_chickpoint_nettoworld = Color.yellow;

    public static float length_chickpoint = 0.2f;

    void Update()
    {
        if (bool_drawpoint_chick && Input.GetMouseButtonDown(0))
        {
            // 将鼠标点击的屏幕坐标转换为世界坐标
            Vector3 wPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z));
            Debug.Log("鼠标点击的屏幕坐标转换为世界坐标" + wPos);
            // 将世界坐标转换为瓦片坐标
            Vector3Int cellPos = grid_.WorldToCell(wPos);
            // 由于是2D, 所以手动将瓦片的Z坐标改为0, 实际项目可以先查看瓦片的Z坐标值.
            cellPos.z = 0;
            Debug.Log("世界坐标转换为瓦片坐标" + cellPos);

            Vector2Int netPos = asGrid.GridP_reflect_NetP(new Vector2Int(cellPos.x, cellPos.y));
            if (!AsGrid.Instance.If_OverRange(netPos.x, netPos.y))
            {
                Vector3 cellPos_World_net = asGrid.nodeslocationes[netPos.x, netPos.y];
                Debug.Log("点击坐标nettoworld" + cellPos_World_net);
                Debug_DrawChickPoint(cellPos_World_net, color_debug_chickpoint_nettoworld);

                Vector3 cellPos_World_ctw = grid_.CellToWorld(cellPos);
                Debug.Log("点击坐标celltoworld" + cellPos_World_ctw);
                Debug_DrawChickPoint(cellPos_World_ctw, color_debug_chickpoint_celltoworld);
                if (bool_findpathmove_chick)
                {
                    basicUnit_test.SetMoveOnAsNodeNet(netPos);
                }
                if (bool_findpathmove_chick2)
                {
                    basicUnit_test2.if_selectpath_auto = true;
                    basicUnit_test2.targetpos = new Vector2Int(netPos.x, netPos.y);
                }
            }
            
        }

    }
    public static void Debug_DrawChickPoint(Vector3 maskpos, Color color_debug_chickpoint)
    {
        Vector3 p1 = new Vector3(maskpos.x, maskpos.y + length_chickpoint * 1.4f, maskpos.z);
        Vector3 p2 = new Vector3(maskpos.x + length_chickpoint * 1.4f, maskpos.y, maskpos.z);
        Vector3 p3 = new Vector3(maskpos.x, maskpos.y - length_chickpoint * 1.4f, maskpos.z);
        Vector3 p4 = new Vector3(maskpos.x - length_chickpoint * 1.4f, maskpos.y, maskpos.z);
        Vector3 p5 = new Vector3(maskpos.x + length_chickpoint, maskpos.y + length_chickpoint, maskpos.z);
        Vector3 p6 = new Vector3(maskpos.x + length_chickpoint, maskpos.y - length_chickpoint, maskpos.z);
        Vector3 p7 = new Vector3(maskpos.x - length_chickpoint, maskpos.y + length_chickpoint, maskpos.z);
        Vector3 p8 = new Vector3(maskpos.x - length_chickpoint, maskpos.y - length_chickpoint, maskpos.z);
        Debug.DrawLine(p1, p2, color_debug_chickpoint, 2); Debug.DrawLine(p2, p3, color_debug_chickpoint, 2);
        Debug.DrawLine(p3, p4, color_debug_chickpoint, 2); Debug.DrawLine(p4, p1, color_debug_chickpoint, 2);
        Debug.DrawLine(p5, p6, color_debug_chickpoint, 2); Debug.DrawLine(p6, p7, color_debug_chickpoint, 2);
        Debug.DrawLine(p7, p8, color_debug_chickpoint, 2); Debug.DrawLine(p8, p5, color_debug_chickpoint, 2);
    }
}
