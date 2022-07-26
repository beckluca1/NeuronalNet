class sign
{
    lowerPointX = 0;
    lowerPointY = 0;
    upperPointX = 0;
    upperPointY = 0;
    lower = true;
    constructor(lowerPointX,lowerPointY,upperPointX,upperPointY)
    {
        this.lowerPointX = lowerPointX;
        this.owerPointY = lowerPointY;
        this.upperPointX = upperPointX;
        this.upperPointY = upperPointY;
    }

    distance()
    {
        let distance1 = Math.pow(this.lowerPointX-MOUSE_CURSOR_POSITION_X,2)+Math.pow(this.lowerPointY-MOUSE_CURSOR_POSITION_Y,2);
        let distance2 = Math.pow(this.upperPointX-MOUSE_CURSOR_POSITION_X,2)+Math.pow(this.upperPointY-MOUSE_CURSOR_POSITION_Y,2);
        this.lower = distance1<distance2;
        return this.lower ? distance1 : distance2;
    }

    move()
    {
        if(this.lower)
        {
            this.lowerPointX = Math.min(MOUSE_CURSOR_POSITION_X+MDX,this.upperPointX-20);
            this.lowerPointY = Math.min(MOUSE_CURSOR_POSITION_Y+MDY,this.upperPointY-20);
        }
        else
        {
            this.upperPointX = Math.max(MOUSE_CURSOR_POSITION_X+MDX,this.lowerPointX+20);
            this.upperPointY = Math.max(MOUSE_CURSOR_POSITION_Y+MDY,this.lowerPointY+20);       
        }
    }

    draw()
    {
        if(this==signRect[currentSign])
        {
            ctx.fillStyle = "rgba(80,150,190,1)";
            ctx.strokeStyle = "rgba(80,150,190,1)";
        }
        else if(this==signRect[importantSign])
        {
            ctx.fillStyle = "rgba(40,90,110,1)";
            ctx.strokeStyle = "rgba(40,90,110,1)";
        }
        else
        {
            ctx.fillStyle = "rgba(0,0,0,1)";
            ctx.strokeStyle = "rgba(0,0,0,1)";
        }

        ctx.beginPath()
        ctx.rect(this.lowerPointX, this.lowerPointY, this.upperPointX-this.lowerPointX, this.upperPointY-this.lowerPointY);
        ctx.stroke();

        ctx.beginPath();
        ctx.arc(this.upperPointX, this.upperPointY, 10, 0, 2 * Math.PI);
        ctx.fill();
    
        ctx.beginPath();
        ctx.arc(this.lowerPointX, this.lowerPointY, 10, 0, 2 * Math.PI);
        ctx.fill();
    
        if(this==signRect[currentSign])
            ctx.fillStyle = "rgba(80,150,190,0.3)";
        else if(this==signRect[importantSign])
            ctx.fillStyle = "rgba(40,90,110,0.3)";
        else
            ctx.fillStyle = "rgba(0,0,0,0.3)";

        ctx.beginPath()
        ctx.rect(this.lowerPointX, this.lowerPointY, this.upperPointX-this.lowerPointX, this.upperPointY-this.lowerPointY);
        ctx.fill();

    }
}

var canvas;
var ctx;

var replacedImage = false;

var canvasWidth = 420;
var canvasHeight = 420;
var extraWidth = 400;

var signRect = [];
var currentSign;
var importantSign;

var MOUSE_CURSOR_POSITION_X = 0;
var MOUSE_CURSOR_POSITION_Y = 0;

var MDX = 0;
var MDY = 0;

var MOUSE_CLICK = false;
var MOUSE_DOWN = false;

var signImage = null;
var trafficImage = null;

var netValues = [0,0,0,0,0];

function init()
{
    canvas = document.getElementById('image');
    canvas.width = canvasWidth+extraWidth;
    canvas.height = canvasHeight;

    ctx = canvas.getContext('2d');

    document.getElementById('files').addEventListener('change', dateiauswahl, false);
    document.addEventListener("mousemove", getMouseCoords);
    document.addEventListener("mousedown", mouseDown);
    document.addEventListener("mouseup", mouseRelease);

    setInterval(intervalCall, 100);
}

function getMouseCoords(event) 
{
    let xx = event.clientX-canvas.getBoundingClientRect().left;
    let yy = event.clientY-canvas.getBoundingClientRect().top;

    MOUSE_CURSOR_POSITION_X = Math.min(Math.max(xx,0),canvas.width-extraWidth);
    MOUSE_CURSOR_POSITION_Y = Math.min(Math.max(yy,0),canvas.height);
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
    var img = document.getElementById('list');
    img.hidden = true;

    if(!img.width>0)
        return;

    let factor = Math.min((canvas.width-extraWidth)/img.width,canvas.height/img.height);

    if(importantSign!=null)
    {
        let signImageWidth = signRect[importantSign].upperPointX-signRect[importantSign].lowerPointX;
        let signImageHeight = signRect[importantSign].upperPointY-signRect[importantSign].lowerPointY;
        let signFactorX = 48/signImageWidth;
        let signFactorY = 48/signImageHeight;
        let signFactorW = signFactorX*factor;
        let signFactorH = signFactorY*factor;
    
        ctx.drawImage(img, -signRect[importantSign].lowerPointX*signFactorX, -signRect[importantSign].lowerPointY*signFactorY, img.width*signFactorW, img.height*signFactorH);
    
        signImage = ctx.getImageData(0, 0, 48, 48);
    }


    ctx.clearRect(0,0,canvas.width,canvas.height);

    let trafficImageWidth = canvasWidth;
    let trafficImageHeight = canvasHeight;
    let trafficFactorW = 48/trafficImageWidth*factor;
    let trafficFactorH = 48/trafficImageHeight*factor;

    ctx.drawImage(img, 0, 0, img.width*trafficFactorW, img.height*trafficFactorH);

    trafficImage = ctx.getImageData(0, 0, 48, 48);

    if(importantSign!=null)
    {

        let factorS = Math.floor(canvasHeight/48/4);

        for (var x=0;x<48;x++)
        {
            for (var y=0;y<48;y++)
            {
                ctx.fillStyle = "rgba("+signImage.data[0+x*4+y*4*48]+",0,0,1)";
                ctx.beginPath()
                ctx.rect(canvasWidth+20+x*factorS, y*factorS, factorS, factorS);
                ctx.fill();
                ctx.fillStyle = "rgba(0,"+signImage.data[1+x*4+y*4*48]+",0,1)";
                ctx.beginPath()
                ctx.rect(canvasHeight+20+x*factorS, y*factorS+48*factorS, factorS, factorS);
                ctx.fill();
                ctx.fillStyle = "rgba(0,0,"+signImage.data[2+x*4+y*4*48]+",1)";
                ctx.beginPath()
                ctx.rect(canvasWidth+20+x*factorS, y*factorS+96*factorS, factorS, factorS);
                ctx.fill();
                ctx.fillStyle = "rgba("+signImage.data[0+x*4+y*4*48]+","+signImage.data[1+x*4+y*4*48]+","+signImage.data[2+x*4+y*4*48]+",1)";
                ctx.beginPath()
                ctx.rect(canvasWidth+20+x*factorS, y*factorS+144*factorS, factorS, factorS);
                ctx.fill();
            }
        }
    }

    ctx.drawImage(img, 0, 0, img.width*factor, img.height*factor);

    if(MOUSE_CLICK&&MOUSE_CURSOR_POSITION_X!=null&&MOUSE_CURSOR_POSITION_Y!=null&&signRect.length>0)
    {
        var minDistance = signRect[0].distance();
        var minIndex = 0;
        for(var i=0;i<signRect.length;i++)
        {
            var distance = signRect[i].distance();
            minIndex = distance<minDistance ? i : minIndex;
            minDistance = distance<minDistance ? distance : minDistance;
        }
        if(minDistance<200)
        {
            currentSign = minIndex;
            importantSign = currentSign;
            MDX = (signRect[currentSign].lower ? signRect[currentSign].lowerPointX : signRect[currentSign].upperPointX) - MOUSE_CURSOR_POSITION_X;
            MDY = (signRect[currentSign].lower ? signRect[currentSign].lowerPointY : signRect[currentSign].upperPointY) - MOUSE_CURSOR_POSITION_Y;
        }
        else
        {
            currentSign = null;
            MDX = 0;
            MDY = 0;
        }
    }

    if(MOUSE_DOWN&&MOUSE_CURSOR_POSITION_X!=null&&MOUSE_CURSOR_POSITION_Y!=null&&currentSign!=null)
    {
        signRect[currentSign].move();
    }
    
    for(var i=0;i<signRect.length;i++)
    {
        signRect[i].draw();
    }

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

    MOUSE_CLICK = false;
}

function addSign()
{
    signRect.push(new sign(0,0,canvas.width-extraWidth,canvas.height));
    currentSign = signRect.length-1;
    importantSign = currentSign;
    signRect[currentSign].selected = true;
}

function removeSign()
{
    if(signRect.length>0&&importantSign!=null)
    {
        for(var i=importantSign;i<signRect.length-1;i++)
        {
            signRect[i] = signRect[i+1];
        }
        signRect.pop();
        if(signRect.length>0)
            importantSign = signRect.length-1;
        else
            importantSign = null;
        currentSign = null;
    }

}

function callNetUpdate()
{
    if(signImage==null)
        return;

    let size = 48;

    var signImageData = [];
    signImageData.push(size);

    for (var y=0;y<48;y++)
    {
        for (var x=0;x<48;x++)
        {
            signImageData.push(signImage.data[0+x*4+y*4*48]);
            signImageData.push(signImage.data[1+x*4+y*4*48]);
            signImageData.push(signImage.data[2+x*4+y*4*48]);
        }
    }

    DotNet.invokeMethod('NeuronalNetClient', 'UpdateNetImage',signImageData); 
}

function updateNet(values)
{
    netValues = values;
}

function uploadImage(number)
{
    let size = 48;

    var signImageData = [];
    signImageData.push(size);
    signImageData.push(number);

    var trafficImageData = [];
    trafficImageData.push(size);
    trafficImageData.push(signRect.length);

    for (var x=0;x<48;x++)
    {
        for (var y=0;y<48;y++)
        {
            signImageData.push(signImage.data[0+x*4+y*4*48]);
            signImageData.push(signImage.data[1+x*4+y*4*48]);
            signImageData.push(signImage.data[2+x*4+y*4*48]);

            trafficImageData.push(trafficImage.data[0+x*4+y*4*48]);
            trafficImageData.push(trafficImage.data[1+x*4+y*4*48]);
            trafficImageData.push(trafficImage.data[2+x*4+y*4*48]);
        }
    }

    for (var i=0;i<signRect.length;i++)
    {
        let x = Math.floor((signRect[i].lowerPointX+signRect[i].upperPointX)/2/canvasWidth*255);
        let y = Math.floor((signRect[i].lowerPointY+signRect[i].upperPointY)/2/canvasHeight*255);
        let w = Math.floor((signRect[i].upperPointX-signRect[i].lowerPointX)/canvasWidth*255);
        let h = Math.floor((signRect[i].upperPointY-signRect[i].lowerPointY)/canvasHeight*255);

        trafficImageData.push(x);
        trafficImageData.push(y);
        trafficImageData.push(w);
        trafficImageData.push(h);
    }

    DotNet.invokeMethod('NeuronalNetClient', 'UploadSignImageData', signImageData); 
    DotNet.invokeMethod('NeuronalNetClient', 'UploadTrafficImageData', trafficImageData); 

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
            signRect = [];
            currentSign = null;
            importantSign = null;
        };
        })(f);
        // Bilder als Data URL auslesen.
        reader.readAsDataURL(f);
    }
}