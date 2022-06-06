# tilemap-A-pathfinder
个人自制的unity A* 寻路的脚本，脚本可以将grid下的任意的tilemap处理为地图数据，并可以返回tilemap上任意两点之间的最短数据
## 使用方式
### 网格化地图
打开Assert里的Script文件夹，将脚本挂载到场景中，然后将Grid中你想要网格化的tilemap拖入Map属性中

![image](https://user-images.githubusercontent.com/59394537/172198917-25722e2f-782d-4a0b-893c-0dc4c97a37e8.png)

将场景运行，脚本自动会将tilemap转化为网格数据，启用时，将bool_net_debug置为true后，你就可以可以看到窗口中被可视化显示的地图网格，红色的是不能走的，白色的是可走的，

![image](https://user-images.githubusercontent.com/59394537/172200807-16ebbec9-84e4-4f42-95b3-09bf06032390.png)


你也可以调节
```
    color_debug_net;
    color_debug_mask;
    color_debug_path;
```
三个属性改变网格的颜色

### 代表网格坐标的数据结构(网格坐标系是在tilemap生成的网格中以最左边和最下边的格子为00坐标，上为y轴正方向，向右边为x轴正方向)

```
    public class AsNode
{
    public int x;//储存网格坐标系的x
    public int y;//储存网格坐标系的y

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
```

#### 网格坐标系转化为世界坐标

```
    public Vector2[,] nodeslocationes;
```
储存了每个AsNode在世界坐标系对应的坐标，nodeslocationes[AsNode.x,AsNode.y]取出AsNode的世界坐标

#### 世界坐标转化为网格坐标

```
    Vector3Int unitPosN3=AsGrid.Instance.map.WorldToCell(/*脚本世界坐标*/transform.position);//世界坐标转化为tilemap坐标
    Vector2Int unitPosN2 = new Vector2Int(unitPosN3.x, unitPosN3.y);
    unitPosN2 = AsGrid.Instance.GridP_reflect_NetP(unitPosN2);//tilemap坐标转化为网格坐标
```

### 获取网格上两点的最短距离

#### 代码接口获取最短路径

```
public List<AsNode> FindPath(Vector2 starP, Vector2 endP)

public List<AsNode> FindPath(Vector2Int starP, Vector2Int endP)
```
利用AsGrid的函数FindPath，向函数输入二维向量（坐标系是以最左边和最下边为00坐标，上为y轴正方向，向右边为x轴正方向），获取包含开头和结尾的List<AsNode>路径
  
## 测试用例
Example场景，当你点击到任意tilemap的小格子中，那么testunit将自动寻路到点击处，同时画出寻路终点和寻路路径，如何利用AsGrid请自行到脚本中查看

  
蓝色的圆圈和线条是路径和目标点还有当前游戏对象移动到的点
  
![スクリーンショット 2022-06-07 004241](https://user-images.githubusercontent.com/59394537/172206583-c51bdce0-c317-4a0e-a212-5a4a51887fa4.png)

  
