var canvas;
var ctx;

var replacedImage = false;

var canvasWidth = 420;
var canvasHeight = 420;
var extraWidth = 400;

var lowerPointX = 0;
var lowerPointY = 0;
var upperPointX = 0;
var upperPointY = 0;

var MOUSE_CURSOR_POSITION_X = 0;
var MOUSE_CURSOR_POSITION_Y = 0;

var MOUSE_CLICK = false;
var MOUSE_DOWN = false;

var myData = null;

var netValues = [0,0,0,0,0];

function init()
{
    canvas = document.getElementById('image');
    canvas.width = canvasWidth+extraWidth;
    canvas.height = canvasHeight;
    upperPointX = canvas.width-extraWidth;
    upperPointY = canvas.height;

    ctx = canvas.getContext('2d');

    document.getElementById('files').addEventListener('change', dateiauswahl, false);
    document.addEventListener("mousemove", getMouseCoords);
    document.addEventListener("mousedown", mouseDown);
    document.addEventListener("mouseup", mouseRelease);

    setInterval(intervalCall, 100);
    setInterval(callNetUpdate, 10000);

}

function getMouseCoords(event) 
{
    let xx = event.clientX-canvas.getBoundingClientRect().left;
    let yy = event.clientY-canvas.getBoundingClientRect().top;

    if(xx>=0
        && xx<=canvas.width-extraWidth
        && yy>=0
        && yy<=canvas.height)
    {
        MOUSE_CURSOR_POSITION_X = Math.min(Math.max(xx,0),canvas.width-extraWidth);
        MOUSE_CURSOR_POSITION_Y = Math.min(Math.max(yy,0),canvas.height);
    }
    else
    {
        MOUSE_CURSOR_POSITION_X = null;
        MOUSE_CURSOR_POSITION_Y = null;
    }
}
  
function mouseDown(event) 
{
    MOUSE_CLICK = true;
    MOUSE_DOWN = true;
}
  
function mouseRelease(event) 
{
    MOUSE_DOWN = false;
    MOUSE_CLICK = false;
  }

function intervalCall()
{
    ctx.clearRect(0,0,canvas.width,canvas.height);

    var img = document.getElementById('list');
    img.hidden = true;

    if(!img.width>0)
        return;

    let factor = Math.min((canvas.width-extraWidth)/img.width,canvas.height/img.height);

    let newImageWidth = upperPointX-lowerPointX;
    let newImageHeight = upperPointY-lowerPointY;
    let newFactorX = 46/newImageWidth;
    let newFactorY = 46/newImageHeight;
    let newFactorW = 46/newImageWidth*factor;
    let newFactorH = 46/newImageHeight*factor;

    ctx.drawImage(img, -lowerPointX*newFactorX, -lowerPointY*newFactorY, img.width*newFactorW, img.height*newFactorH);

    myData = ctx.getImageData(0, 0, 46, 46);

    let factorS = Math.floor(canvasHeight/46/4);

    for (var x=0;x<46;x++)
    {
        for (var y=0;y<46;y++)
        {
            ctx.fillStyle = "rgba("+myData.data[0+x*4+y*4*46]+",0,0,1)";
            ctx.beginPath()
            ctx.rect(canvasWidth+20+x*factorS, y*factorS, factorS, factorS);
            ctx.fill();
            ctx.fillStyle = "rgba(0,"+myData.data[1+x*4+y*4*46]+",0,1)";
            ctx.beginPath()
            ctx.rect(canvasHeight+20+x*factorS, y*factorS+46*factorS, factorS, factorS);
            ctx.fill();
            ctx.fillStyle = "rgba(0,0,"+myData.data[2+x*4+y*4*46]+",1)";
            ctx.beginPath()
            ctx.rect(canvasWidth+20+x*factorS, y*factorS+92*factorS, factorS, factorS);
            ctx.fill();
            ctx.fillStyle = "rgba("+myData.data[0+x*4+y*4*46]+","+myData.data[1+x*4+y*4*46]+","+myData.data[2+x*4+y*4*46]+",1)";
            ctx.beginPath()
            ctx.rect(canvasWidth+20+x*factorS, y*factorS+138*factorS, factorS, factorS);
            ctx.fill();
        }
    }

    ctx.drawImage(img, 0, 0, img.width*factor, img.height*factor);

    if(MOUSE_DOWN&&MOUSE_CURSOR_POSITION_X!=null&&MOUSE_CURSOR_POSITION_Y!=null)
    {
        let distance1 = Math.pow(lowerPointX-MOUSE_CURSOR_POSITION_X,2)+Math.pow(lowerPointY-MOUSE_CURSOR_POSITION_Y,2)
        let distance2 = Math.pow(upperPointX-MOUSE_CURSOR_POSITION_X,2)+Math.pow(upperPointY-MOUSE_CURSOR_POSITION_Y,2)

        if(distance1<distance2)
        {
            lowerPointX = MOUSE_CURSOR_POSITION_X;
            lowerPointY = MOUSE_CURSOR_POSITION_Y;
        }
        else
        {
            upperPointX = MOUSE_CURSOR_POSITION_X;
            upperPointY = MOUSE_CURSOR_POSITION_Y;            
        }

    }


    ctx.fillStyle = "rgba(0,0,0,1)";
    ctx.beginPath();
    ctx.arc(upperPointX, upperPointY, 10, 0, 2 * Math.PI);
    ctx.fill();

    ctx.beginPath();
    ctx.arc(lowerPointX, lowerPointY, 10, 0, 2 * Math.PI);
    ctx.fill();

    ctx.beginPath()
    ctx.rect(lowerPointX, lowerPointY, upperPointX-lowerPointX, upperPointY-lowerPointY);
    ctx.stroke();

    var max = 255*netValues[0];
    var maxIndex = 0;

    for(var i=0;i<netValues.length;i++)
    {
        let valueC = 255*netValues[i];
        maxIndex = valueC > max ? i : maxIndex;
        max = valueC > max ? valueC : max;
        ctx.fillStyle = "rgba("+valueC+","+valueC+","+valueC+",1)";
        ctx.beginPath();
        ctx.arc(canvasWidth+140, 10+i*20, 8, 0, 2 * Math.PI);
        ctx.fill();
    }

    ctx.fillStyle = "rgba(0,0,0,1)";
    ctx.beginPath();
    ctx.arc(canvasWidth+140, 10+maxIndex*20, 8, 0, 2 * Math.PI);
    ctx.stroke();

    var text;
    if(maxIndex==0)
        text = "Stoppschild"
    else if(maxIndex==1)
        text = "Tempolimit 30"
    else if(maxIndex==2)
        text = "Tempolimit 50"
    else if(maxIndex==3)
        text = "Vorfahrtsstraße"
    else if(maxIndex==4)
        text = "Vorfahrt gewähren"

    ctx.font = "10px Georgia";
    ctx.fillText(text, canvasWidth+155, 12+maxIndex*20);

}

function callNetUpdate()
{
    if(myData==null)
        return;

    let size = 46;

    var allData = [];
    allData.push(size);

    for (var y=0;y<46;y++)
    {
        for (var x=0;x<46;x++)
        {
            allData.push(myData.data[0+x*4+y*4*46]);
            allData.push(myData.data[1+x*4+y*4*46]);
            allData.push(myData.data[2+x*4+y*4*46]);
        }
    }

    DotNet.invokeMethod('NeuronalNetClient', 'GetNetData',allData); 
}

function updateNet(values)
{
    netValues = values;
}

function fillCanvas(number)
{
    let size = 46;

    var allData = [];
    allData.push(size);
    allData.push(number);

    for (var x=0;x<46;x++)
    {
        for (var y=0;y<46;y++)
        {
            allData.push(myData.data[0+x*4+y*4*46]);
            allData.push(myData.data[1+x*4+y*4*46]);
            allData.push(myData.data[2+x*4+y*4*46]);
        }
    }

    DotNet.invokeMethod('NeuronalNetClient', 'GetImageData', allData); 
}

function dateiauswahl(evt) {
    var dateien = evt.target.files; // FileList object
    // Auslesen der gespeicherten Dateien durch Schleife
    f = dateien[0];
    // nur Bild-Dateien
    if (f.type.match('image.*')) 
    {
        var reader = new FileReader();
        reader.onload = (function (theFile) {
        return function (e) {
            // erzeuge Thumbnails.
            var vorschau = document.createElement('img');
            vorschau.src = e.target.result;
            vorschau.id = 'list';
            document.getElementById('list').replaceWith(vorschau);
            replacedImage = false;
        };
        })(f);
        // Bilder als Data URL auslesen.
        reader.readAsDataURL(f);
    }
}
