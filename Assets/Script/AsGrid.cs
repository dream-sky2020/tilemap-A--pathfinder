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

    public float f;//Ѱ·����
    public float g;//�����ľ���
    public float h;//���յ�ľ���
    public AsNode father;

    public E_Node_Type type;//���ӵ�����

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

    private Vector3 GridOrigionPos;//����ԭ�����

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


    

    public AsNode[,] nodes;//��ͼ��ص����н��
    private List<AsNode> openList = new List<AsNode>();//�����б�
    private List<AsNode> closeList = new List<AsNode>();//�ر��б�

    public void InitMapInfo(int w, int h) //���ݿ�ߴ�������
    {
        Debug.Log("InitMapInfo���ݿ�ߴ�������");
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
    public List<AsNode> FindPath(Vector2 starP, Vector2 endP) //Ѱ·����Vector2���ͣ��ṩ�ⲿʹ��
    {
        Vector2Int starPInt = new Vector2Int((int)starP.x, (int)starP.y);
        Vector2Int endPInt = new Vector2Int((int)endP.x, (int)endP.y);
        return FindPath(starPInt, endPInt);
    }
    public List<AsNode> FindPath(Vector2Int starP, Vector2Int endP) //Ѱ·����Vector2Int���ͣ��ṩ�ⲿʹ��
    {
        if (starP.Equals(endP))
        {
            Debug.LogWarning("��ʼ����ǽ�����");
            return null;
        }
        //���������û�г���Χ
        if (If_OverRange((int)starP.x, (int)starP.y, nodes.GetLength(0), nodes.GetLength(1)) ||
            If_OverRange((int)endP.x, (int)endP.y, nodes.GetLength(0), nodes.GetLength(1)))
        {
            Debug.LogWarning("��ʼ���ǽ������ڵ�ͼ������");
            return null;
        }
        AsNode start = nodes[(int)starP.x, (int)starP.y];
        AsNode end = nodes[(int)endP.x, (int)endP.y];

        closeList.Clear();
        openList.Clear();
        //��չرպͿ����б�

        start.father = null;
        start.f = 0;
        start.g = 0;
        start.h = 0;
        //��ʼ����ʼ��

        closeList.Add(start);
        //����ʼ�����ر��б���

        while (true)
        {
            Debug.Log("����Χ�ĵ��ҵ������뿪���б���");
            //����㿪ʼ������Χ�ĵ��ҵ������뿪���б���
            Debug.Log("�����ϵĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x - 1, start.y - 1, 1.4f, start, end);
            //����
            Debug.Log("���ϵĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x, start.y - 1, 1f, start, end);
            //��
            Debug.Log("�����ϵĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x + 1, start.y - 1, 1.4f, start, end);
            //����
            Debug.Log("����ĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x - 1, start.y, 1f, start, end);
            //��
            Debug.Log("���ҵĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x + 1, start.y, 1f, start, end);
            //��
            Debug.Log("�����µĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x - 1, start.y + 1, 1.4f, start, end);
            //����
            Debug.Log("���µĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x, start.y + 1, 1f, start, end);
            //��
            Debug.Log("�����µĵ��ҵ������뿪���б���");
            FindNearlyNodeToOpenList(start.x + 1, start.y + 1, 1.4f, start, end);
            //����

            //��������б��ǿյģ���û���յ㣬������·
            if (openList.Count == 0)
            {
                Debug.LogWarning("�����б�Ϊ�գ���·���ܣ�Ҳ�п�����");
                return null;
            }

            //ѡ�п����б���Ѱ·������С�ĵ�
            openList.Sort(SortOpenList);

            //����ر��б��У�Ȼ��ӿ����б����Ƴ�
            closeList.Add(openList[0]);
            //�ҵ�����㣬������һ����Ѱ·����
            start = openList[0];
            openList.RemoveAt(0);
            //�����������յ㣬��ô�������ս�������û�У�����Ѱ·
            if (start == end)
            {
                //�ҵ���
                List<AsNode> path = new List<AsNode>();
                path.Add(end);//���յ�ѹ��ȥ
                while (end.father != null)//һֱѹ�����
                {
                    path.Add(end.father);
                    end = end.father;
                }
                path.Reverse();//�б�ת

                return path;
            }
        }



    }
    public bool If_OverRange(int x, int y, int xw, int yh) //��������x��y�Ƿ񳬳�xw��0��yh��0�ķ�Χ��Ҫ����0<=x<max��0<=y<max
    {
        if (x < 0 || x >= xw ||
            y < 0 || y >= yh)
        {
            Debug.LogWarning("Խ�羯��:" + "����" + x + " " + y + "�������飺��" + xw + "��" + yh);
            return true;
        }
        return false;
    }
    public bool If_OverRange(int x, int y) 
    {
        if (x < 0 || x >= nodes.GetLength(0) ||
            y < 0 || y >= nodes.GetLength(1))
        {
            Debug.LogWarning("Խ�羯��:" + "����" + x + " " + y + "�������飺��" + nodes.GetLength(0) + "��" + nodes.GetLength(1));
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
        //�ж���Щ���Ƿ��ڱ߽磬�Ƿ��赲���Ƿ�

        //����fֵ f=g+h

        //��¼������
        node.father = father;
        //����g �������ľ�������Ҹ��������ľ�����������Ҹ��׵ľ���
        node.g = father.g + g;
        node.h = Mathf.Abs(end.x - node.x) + Mathf.Abs(end.y - node.y);
        node.f = node.g + node.h;
        //�����������֤�� �浽�����б���
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
        Debug.Log("�˽⵽��tile��ͼ�Ŀ��Ϊ" + map_width + "�߶�Ϊ" + map_height);

        map_width_min = map.cellBounds.xMin;
        map_width_max = map.cellBounds.xMax;
        map_height_min = map.cellBounds.yMin;
        map_height_max = map.cellBounds.yMax;//��Ҳû�취��˭��������������Ҫ�ȶ�һ��1
        Debug.Log("�˽⵽��tile��ͼ��x������Ϊ" + map_width_min + "������Ϊ" + map_width_max + "�˽⵽��tile��ͼ��y������Ϊ" + map_height_min + "������Ϊ" + map_height_max);

        map_center = map.cellBounds.center;

        Debug.Log("�˽⵽��tile��ͼ�����ĵ�λ��Ϊ" + map_center);

        AsGrid.Instance.InitMapInfo(map_width, map_height);
        Debug.Log("����instance�ĵ����");

        GridOrigionPos = map.GetCellCenterWorld(new Vector3Int(-map_width / 2, -map_height / 2, 0));
        Debug.Log("��δ��ɣ��õ��յ�λ��Ϊ" + GridOrigionPos);
        nodeslocationes = new Vector2[map_width, map_height];

        for (int x = 0, i = map_width_min; i < map_width_max; x++, i++)
        {
            for (int y = 0, j = map_height_min; j < map_height_max; y++, j++)
            {
                Debug.Log("��ȡ��ӳtile�����Ӧ�ĸ��ӵ���������");
                nodeslocationes[x, y] = map.GetCellCenterWorld(new Vector3Int(i, j, 0));
                Debug.Log("��ȡ��ӳtile�����Ӧ�ĸ��Ӵ���");
                TileBase tile = map.GetTile(new Vector3Int(i, j, 0));
                AsGrid.Instance.nodes[x, y].type = tile == null ? E_Node_Type.Stop : E_Node_Type.Walk;
                Debug.Log("��nodeslocationes����tile����λ�ö�Ӧʵ������λ�ñ��е�" + i + " " + j + "����������λ��" + map.GetCellCenterWorld(new Vector3Int(i, j, 0)));
                string messagewalkable;
                if (AsGrid.Instance.nodes[x, y].type == E_Node_Type.Walk) messagewalkable = "�����߶�";
                else messagewalkable = "�����߶�";
                Debug.Log("��nodeslocationes����tile����λ�ö�Ӧʵ������λ�ñ��е�" + i + " " + j + "������������Ϣ" + messagewalkable);
            }
        }

        GridSize = nodeslocationes[0, 0].x - nodeslocationes[1, 0].x;

        Debug.Log("�����ʼ���");
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
            Vector3 pos2 = map.GetCellCenterWorld(new Vector3Int(map_width_max - 1, j, 0));//�м�Ҫ-1
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
