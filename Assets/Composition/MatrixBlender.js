@script RequireComponent (typeof(Camera))

 
private var ortho: Matrix4x4;
private var perspective: Matrix4x4;
           
public var fov: float   = 60f;
public var near: float  = 0.5f;
public var far: float   = 50f;
public var orthographicSize: float = 20f;
 
private var aspect: float;
private var orthoOn: boolean;
 
function Awake ()
{
    aspect = (Screen.width+0.0) / (Screen.height+0.0);
 
    perspective = camera.projectionMatrix;
 
    ortho = Matrix4x4.Ortho(-orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, near, far); 
    orthoOn = false;
}
 
function Update ()
{
    if ( Input.GetKeyDown(KeyCode.P) )
    {
        orthoOn = !orthoOn;
        if (orthoOn)
            BlendToMatrix(ortho, 1f);
        else
            BlendToMatrix(perspective, 1f);
    }
}
 
 
 
static function MatrixLerp (from: Matrix4x4, to: Matrix4x4, time: float) : Matrix4x4
{
    var ret: Matrix4x4 = new Matrix4x4();
    var i: int;
    for (i = 0; i < 16; i++)
        ret[i] = Mathf.Lerp(from[i], to[i], time);
    return ret;
}
 
private function LerpFromTo (src: Matrix4x4, dest: Matrix4x4, duration: float) : IEnumerator
{
    var startTime: float = Time.time;
    while (Time.time - startTime < duration)
    {
        camera.projectionMatrix = MatrixLerp(src, dest, (Time.time - startTime) / duration);
        yield;
    }
    camera.projectionMatrix = dest;
}
 
public function BlendToMatrix (targetMatrix: Matrix4x4, duration: float) : Coroutine
{
    StopAllCoroutines();
    return StartCoroutine(LerpFromTo(camera.projectionMatrix, targetMatrix, duration));
}