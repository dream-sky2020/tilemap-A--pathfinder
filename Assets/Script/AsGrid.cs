using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum E_Node_Type
{
    Walk, Stop
}
public class AsNode
{
    public int x;
    public int y;

    public float f;//寻路消耗
    public float g;//离起点的距离
    public float h;//离终点的距离
    public AsNode father;

    public E_Node_Type type;//格子的类型

    public AsNode(int x, int y, E_Node_Type type)
    {
        this.x = x;
        this.y = y;
        this.type = type;
    }
}



public class AsGrid : MonoBehaviour
{
    private bool bool_run = false;

    public Tilemap map;
    private int map_width;
    private int map_height;

    public int map_width_max;
    public int map_height_max;
    public int map_width_min;
    public int map_height_min;

    public Vector3 map_center;


    public float GridSize;

    private Vector3 GridOrigionPos;//格子原点距离

    public Vector2[,] nodeslocationes;

    public float length_mask = 0.5f;
    public Color color_debug_net = Color.green;
    public Color color_debug_mask = Color.red;
    public Color color_debug_path = Color.blue;

    public bool bool_net_debug = false;

    public Color point_walk = Color.white;
    public Color point_stop = Color.red;

    public bool bool_point_debug = false;
    public float CheckrotateDistance = 0.5f;

    private static AsGrid instance;
    public static AsGrid Instance
    {
        get
        {
            if (instance == null)
                instance = new AsGrid();
            return instance;
        }
    }


    

    public AsNode[,] nodes;//地图相关的所有结点
    private List<AsNode> openList = new List<AsNode>();//开启列表
    private List<AsNode> closeList = new List<AsNode>();//关闭列表

    public void InitMapInfo(int w, int h) //根据宽高创建格子
    {
        Debug.Log("InitMapInfo根据宽高创建格子");
        nodes = new AsNode[w, h];
        for (int i = 0; i < w; ++i)
        {
            for (int j = 0; j < h; ++j)
            {
                AsNode node = new AsNode(i, j, E_Node_Type.Walk);
                nodes[i, j] = node;
            }
        }
    }
    public List<AsNode> FindPath(Vector2 starP, Vector2 endP) //寻路方法Vector2类型，提供外部使用
    {
        Vector2Int starPInt = new Vector2Int((int)starP.x, (int)starP.y);
        Vector2Int endPInt = new Vector2Int((int)endP.x, (int)endP.y);
        return FindPath(starPInt, endPInt);
    }
    public List<AsNode> FindPath(Vector2Int starP, Vector2Int endP) //寻路方法Vector2Int类型，提供外部使用
    {
        if (starP.Equals(endP))
        {
            Debug.LogWarning("开始点就是结束点");
            return null;
        }
        //检测坐标有没有出范围
        if (If_OverRange((int)starP.x, (int)starP.y, nodes.GetLength(0), nodes.GetLength(1)) ||
            If_OverRange((int)endP.x, (int)endP.y, nodes.GetLength(0), nodes.GetLength(1)))
        {
            Debug.LogWarning("开始或是结束点在地图网格外");
            return null;
        }
        AsNode start = nodes[(int)starP.x, (int)starP.y];
        AsNode end = nodes[(int)endP.x, (int)endP.y];

        closeList.Clear();
        openList.Clear();
        //清空关闭和开启列表

        start.father = null;
        start.f = 0;
        start.g = 0;
        start.h = 0;
        //初始化开始点

        closeList.Add(start);
        //将开始点放入关闭列表中

        while (true)
        {
            Debug.Log("将周围的点找到并放入开启列表中");
            //从起点开始，将周围的点找到并放入开启列表中
            Debug.Log("将左上的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x - 1, start.y - 1, 1.4f, start, end);
            //左上
            Debug.Log("将上的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x, start.y - 1, 1f, start, end);
            //上
            Debug.Log("将右上的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x + 1, start.y - 1, 1.4f, start, end);
            //右上
            Debug.Log("将左的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x - 1, start.y, 1f, start, end);
            //左
            Debug.Log("将右的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x + 1, start.y, 1f, start, end);
            //右
            Debug.Log("将左下的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x - 1, start.y + 1, 1.4f, start, end);
            //左下
            Debug.Log("将下的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x, start.y + 1, 1f, start, end);
            //下
            Debug.Log("将右下的点找到并放入开启列表中");
            FindNearlyNodeToOpenList(start.x + 1, start.y + 1, 1.4f, start, end);
            //右下

            //如果开启列表都是空的，都没有终点，就是死路
            if (openList.Count == 0)
            {
                Debug.LogWarning("开启列表为空，死路可能，也有可能是");
                return null;
            }

            //选中开启列表中寻路消耗最小的点
            openList.Sort(SortOpenList);

            //放入关闭列表中，然后从开启列表中移除
            closeList.Add(openList[0]);
            //找到这个点，进行下一步的寻路计算
            start = openList[0];
            openList.RemoveAt(0);
            //如果这个点是终点，那么返回最终结果，如果没有，继续寻路
            if (start == end)
            {
                //找到了
                List<AsNode> path = new List<AsNode>();
                path.Add(end);//将终点压进去
                while (end.father != null)//一直压到起点
                {
                    path.Add(end.father);
                    end = end.father;
                }
                path.Reverse();//列表反转

                return path;
            }
        }



    }
    public bool If_OverRange(int x, int y, int xw, int yh) //检测给出的x，y是否超出xw到0和yh到0的范围，要满足0<=x<max和0<=y<max
    {
        if (x < 0 || x >= xw ||
            y < 0 || y >= yh)
        {
            Debug.LogWarning("越界警告:" + "传入" + x + " " + y + "超出数组：宽" + xw + "高" + yh);
            return true;
        }
        return false;
    }
    public bool If_OverRange(int x, int y) 
    {
        if (x < 0 || x >= nodes.GetLength(0) ||
            y < 0 || y >= nodes.GetLength(1))
        {
            Debug.LogWarning("越界警告:" + "传入" + x + " " + y + "超出数组：宽" + nodes.GetLength(0) + "高" + nodes.GetLength(1));
            return true;
        }
        return false;
    }
    private void FindNearlyNodeToOpenList(int x, int y, float g, AsNode father, AsNode end)
    {
        if (If_OverRange((int)x, (int)y, nodes.GetLength(0), nodes.GetLength(1)))
        {
            
            return;
        }
        AsNode node = nodes[x, y];
        if (node == null || node.type == E_Node_Type.Stop || closeList.Contains(node) || openList.Contains(node))
            return;
        //判断这些点是否在边界，是否阻挡，是否

        //计算f值 f=g+h

        //记录父对象
        node.father = father;
        //计算g 我离起点的距离就是我父亲离起点的距离加上我离我父亲的距离
        node.g = father.g + g;
        node.h = Mathf.Abs(end.x - node.x) + Mathf.Abs(end.y - node.y);
        node.f = node.g + node.h;
        //经过上面的验证后 存到开启列表中
        openList.Add(node);


    }
    private int SortOpenList(AsNode a, AsNode b)
    {
        if (a.f > b.f)
            return 1;
        else if (a.f == b.f)
            return 1;
        else
            return -1;
    }
    private void Awake()
    {
        bool_run = true;

        instance = this;

        map_width = map.cellBounds.size.x;
        map_height = map.cellBounds.size.y;
        Debug.Log("了解到了tile地图的宽度为" + map_width + "高度为" + map_height);

        map_width_min = map.cellBounds.xMin;
        map_width_max = map.cellBounds.xMax;
        map_height_min = map.cellBounds.yMin;
        map_height_max = map.cellBounds.yMax;//我也没办法，谁叫他总是正方向要比多一个1
        Debug.Log("了解到了tile地图的x轴的最负端为" + map_width_min + "最正端为" + map_width_max + "了解到了tile地图的y轴的最负端为" + map_height_min + "最正端为" + map_height_max);

        map_center = map.cellBounds.center;

        Debug.Log("了解到了tile地图的中心点位置为" + map_center);

        AsGrid.Instance.InitMapInfo(map_width, map_height);
        Debug.Log("创建instance的点格子");

        GridOrigionPos = map.GetCellCenterWorld(new Vector3Int(-map_width / 2, -map_height / 2, 0));
        Debug.Log("（未完成）得到终点位置为" + GridOrigionPos);
        nodeslocationes = new Vector2[map_width, map_height];

        for (int x = 0, i = map_width_min; i < map_width_max; x++, i++)
        {
            for (int y = 0, j = map_height_min; j < map_height_max; y++, j++)
            {
                Debug.Log("获取对映tile坐标对应的格子的世界坐标");
                nodeslocationes[x, y] = map.GetCellCenterWorld(new Vector3Int(i, j, 0));
                Debug.Log("获取对映tile坐标对应的格子存在");
                TileBase tile = map.GetTile(new Vector3Int(i, j, 0));
                AsGrid.Instance.nodes[x, y].type = tile == null ? E_Node_Type.Stop : E_Node_Type.Walk;
                Debug.Log("向nodeslocationes——tile格子位置对应实际坐标位置表中的" + i + " " + j + "格子填入了位置" + map.GetCellCenterWorld(new Vector3Int(i, j, 0)));
                string messagewalkable;
                if (AsGrid.Instance.nodes[x, y].type == E_Node_Type.Walk) messagewalkable = "可以走动";
                else messagewalkable = "不能走动";
                Debug.Log("向nodeslocationes——tile格子位置对应实际坐标位置表中的" + i + " " + j + "格子填入了信息" + messagewalkable);
            }
        }

        GridSize = nodeslocationes[0, 0].x - nodeslocationes[1, 0].x;

        Debug.Log("网格初始完成");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (bool_net_debug)
            Debug_DrawAsNodeNet();


    }
    public void Debug_DrawAsFpathLine(Vector2Int start, Vector2Int end)
    {
        List<AsNode> listpath = AsGrid.Instance.FindPath(start, end);
        for (int i = 0; i + 1 < listpath.Count; i++)
        {
            AsNode node = listpath[i];
            AsNode node1 = listpath[i + 1];
            Debug.DrawLine(nodeslocationes[node.x, node.y], nodeslocationes[node1.x, node1.y], color_debug_path);
        }
        Vector3 startpos = nodeslocationes[start.x, start.y];
        Vector3 endpos = nodeslocationes[end.x, end.y];
        Debug_DrawAsNodeMask(startpos);
        Debug_DrawAsNodeMask(endpos);
    }
    private void Debug_DrawAsNodeMask(Vector3 maskpos)
    {
        Debug.DrawLine(new Vector3(maskpos.x + length_mask, maskpos.y + length_mask, 0), new Vector3(maskpos.x - length_mask, maskpos.y - length_mask, 0), color_debug_mask);
        Debug.DrawLine(new Vector3(maskpos.x + length_mask, maskpos.y - length_mask, 0), new Vector3(maskpos.x - length_mask, maskpos.y + length_mask, 0), color_debug_mask);

    }
    private void Debug_DrawAsNodeNet()
    {
        for (int i = map_width_min; i < map_width_max; i++)
        {
            Vector3 pos1 = map.GetCellCenterWorld(new Vector3Int(i, map_height_min, 0));
            Vector3 pos2 = map.GetCellCenterWorld(new Vector3Int(i, map_height_max - 1, 0));
            Debug.DrawLine(pos1, pos2, color_debug_net);
        }
        for (int j = map_height_min; j < map_height_max; j++)
        {
            Vector3 pos1 = map.GetCellCenterWorld(new Vector3Int(map_width_min, j, 0));
            Vector3 pos2 = map.GetCellCenterWorld(new Vector3Int(map_width_max - 1, j, 0));//切记要-1
            Debug.DrawLine(pos1, pos2, color_debug_net);
        }
    }
    public Vector2Int GridP_reflect_NetP(Vector2Int gridp)
    {
        return new Vector2Int(gridp.x - map_width_min, gridp.y - map_height_min);
    }
    public void Debug_DrawNodeifWalk()
    {
        foreach (AsNode node in AsGrid.Instance.nodes)
        {
            Vector3 nodepos = new Vector3(AsGrid.Instance.nodeslocationes[node.x, node.y].x, AsGrid.Instance.nodeslocationes[node.x, node.y].y, 0);
            if (node.type == E_Node_Type.Walk)
            {
                Gizmos.color = point_walk;
                Gizmos.DrawWireSphere(nodepos, CheckrotateDistance);
            }
            else
            {
                Gizmos.color = point_stop;
                Gizmos.DrawWireSphere(nodepos, CheckrotateDistance);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (bool_run)
        {
            if (bool_point_debug)
                Debug_DrawNodeifWalk();
        }
    }
}
