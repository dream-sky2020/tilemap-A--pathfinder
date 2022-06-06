using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum StateType_Move
{
    Idle,
    Moving
}
public class TestUnit : MonoBehaviour
{
    private bool bool_run = false;

    public float debug_counttime = 0f;

    public float findpathautotime = 2f;

    [SerializeField] protected StateType_Move statetype_move = StateType_Move.Idle;
    public float OneStepTime = 0.2f;
    protected Transform transform;
    private Vector2Int unitpos;
    public bool if_selectpath_auto = false;
    public Vector2Int targetpos;
    public List<AsNode> findpath;

    public bool bool_drawpath_debug = false;

    public Color color_debug_path = Color.green;
    public float CheckrotateDistance = 0.4f;
    void Awake()
    {
            bool_run = true;
}
    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        InitPosOnNodeNet();
    }

    // Update is called once per frame
    void Update()
    {
        debug_counttime += Time.deltaTime;
        if (if_selectpath_auto && statetype_move == StateType_Move.Idle && targetpos != unitpos)
        {
            Debug.Log("启用寻路函数寻找目标");
            SetMoveOnAsNodeNet(targetpos);
        }
        if (statetype_move == StateType_Move.Idle && findpath != null && findpath.Count > 0)
        {

            Debug.Log("启用协程函数移动");
            statetype_move = StateType_Move.Moving;
            Debug.Log("准备协程函数启用");
            //BasicPathMove();
            bool bool_ifcanmove = true;
            if (findpath == null)
            {
                Debug.Log("基础的单位按路径移动函数接受了一个空路径");
                bool_ifcanmove=false;
            }
            if (findpath.Count == 0)
            {
                Debug.Log("移动路径为空");
                bool_ifcanmove = false;
            }
            //从路径取出下一个移动的点
            AsNode move_node = findpath[0];

            if (move_node.type == E_Node_Type.Stop)
            {
                Debug.Log("移动上有阻挡，停止移动");
                findpath = null;
                bool_ifcanmove = false;
            }
            if (bool_ifcanmove) 
            {
                //移除确认移动的点
                findpath.RemoveAt(0);
                //检查完，开始移动


                //自己的位置让出，占据下一个位置
                AsGrid.Instance.nodes[unitpos.x, unitpos.y].type = E_Node_Type.Walk;//保证原先站立的地方其他人能走
                AsGrid.Instance.nodes[move_node.x, move_node.y].type = E_Node_Type.Stop;//保证下个占据的地方其他人能走

                //逻辑动到指定位置
                Debug.Log("逻辑动到指定位置");
                unitpos.x = move_node.x; unitpos.y = move_node.y;

                Vector3 movepoint_net = AsGrid.Instance.nodeslocationes[move_node.x, move_node.y];

                StartCoroutine(Move_IE(movepoint_net));
            }
        }
    }
    public void InitPosOnNodeNet()
    {
        // 将单位世界坐标转换为瓦片坐标
        Vector3Int unitPosN3 = AsGrid.Instance.map.WorldToCell(transform.position);
        // 由于是2D, 所以手动将瓦片的Z坐标改为0
        Vector2Int unitPosN2 = new Vector2Int(unitPosN3.x, unitPosN3.y);
        Debug.Log("检测到单位的瓦片坐标为" + unitPosN2);
        unitPosN2 = AsGrid.Instance.GridP_reflect_NetP(unitPosN2);
        Debug.Log("检测到单位的瓦片坐标转化为网格坐标为" + unitPosN2);
        transform.position = AsGrid.Instance.nodeslocationes[unitPosN2.x, unitPosN2.y];
        unitpos = unitPosN2;
        AsGrid.Instance.nodes[unitPosN2.x, unitPosN2.y].type = E_Node_Type.Stop;//保证角色站立的地方其他人不能走
    }
    public void SetMoveOnAsNodeNet(Vector2Int trgPos)
    {
        // 将单位世界坐标转换为瓦片坐标
        Vector3Int unitPosN3 = AsGrid.Instance.map.WorldToCell(transform.position);
        // 由于是2D, 所以手动将瓦片的Z坐标改为0
        Vector2Int unitPosN2 = new Vector2Int(unitPosN3.x, unitPosN3.y);
        Debug.Log("检测到单位的瓦片坐标为" + unitPosN2);
        unitPosN2 = AsGrid.Instance.GridP_reflect_NetP(unitPosN2);
        Debug.Log("检测到单位的瓦片坐标转化为网格坐标为" + unitPosN2);
        findpath = AsGrid.Instance.FindPath(unitPosN2, trgPos);
        if (findpath == null)
        {
            Debug.Log("基础的路径获取函数接受了一个空路径");
            return;
        }
        //先瞬移到开始位置，对准方格，取出路径的开始点
        transform.position = AsGrid.Instance.nodeslocationes[unitPosN2.x, unitPosN2.y];
        findpath.RemoveAt(0);
    }
    public IEnumerator Move_IE(Vector3 movepoint_net)
    {
        //实际动到指定位置\
        Debug.Log("实际动到指定位置"+movepoint_net);

        while (transform.position != movepoint_net)
        {
            transform.position = Vector3.MoveTowards(transform.position, movepoint_net, 10 * Time.deltaTime);
            yield return 0;
        }
        statetype_move = StateType_Move.Idle;
        /*
         *         float t = 0;
            *for (; ; )
        {
            Debug.Log("移动中"+t);
            t += Time.deltaTime;
            float a = t / OneStepTime;
            transform.position = Vector3.Lerp(transform.position, movepoint_net, a);
            if (a >= 1.0f)
            {
                Debug.Log("协程函数执行完BasicPathMove后，将移动状态置为待机" + "本次协程函数耗时为" + t);
                statetype_move = StateType_Move.Idle;
                yield break;
            }
            Debug.Log("移动一个轮回");
            yield return null;
        }
        Debug.Log("一定出错了");
        yield break;*/

    }
    private void OnDrawGizmos()
    {
        if (bool_drawpath_debug && findpath != null && findpath.Count >= 0)
        {
            Gizmos.color = color_debug_path;
            for (int i = 0; i + 1 < findpath.Count; i++)
            {

                AsNode node = findpath[i];
                AsNode node1 = findpath[i + 1];
                Gizmos.DrawLine(AsGrid.Instance.nodeslocationes[node.x, node.y], AsGrid.Instance.nodeslocationes[node1.x, node1.y]);
            }
            Vector3 startpos = AsGrid.Instance.nodeslocationes[unitpos.x, unitpos.y];
            Vector3 endpos = AsGrid.Instance.nodeslocationes[targetpos.x, targetpos.y];
            Vector3 localpos = AsGrid.Instance.nodeslocationes[unitpos.x, unitpos.y];
            Gizmos.DrawWireSphere(startpos, CheckrotateDistance);
            Gizmos.DrawWireSphere(endpos, CheckrotateDistance + 0.1f);
            Gizmos.DrawWireSphere(localpos, CheckrotateDistance - 0.1f);
        }
    }
}
