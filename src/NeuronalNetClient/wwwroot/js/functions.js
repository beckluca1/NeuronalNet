function printText()
{
    console.log("Yes");
}

let imageWidth = 5;
let imageHeight = 7;
let pixelSize = 30;

var pixelData = [];
pixelData[0] = [0,0,0,0,0, 0,1,1,1,0, 0,1,0,1,0, 0,1,0,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[1] = [0,0,0,0,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,0,0];
pixelData[2] = [0,0,0,0,0, 0,1,1,1,0, 0,0,0,1,0, 0,1,1,1,0, 0,1,0,0,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[3] = [0,0,0,0,0, 0,1,1,1,0, 0,0,0,1,0, 0,1,1,1,0, 0,0,0,1,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[4] = [0,0,0,0,0, 0,1,0,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,0,0];
pixelData[5] = [0,0,0,0,0, 0,1,1,1,0, 0,1,0,0,0, 0,1,1,1,0, 0,0,0,1,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[6] = [0,0,0,0,0, 0,1,1,1,0, 0,1,0,0,0, 0,1,1,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[7] = [0,0,0,0,0, 0,1,1,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,1,0, 0,0,0,0,0];
pixelData[8] = [0,0,0,0,0, 0,1,1,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,0,0,0,0];
pixelData[9] = [0,0,0,0,0, 0,1,1,1,0, 0,1,0,1,0, 0,1,1,1,0, 0,0,0,1,0, 0,1,1,1,0, 0,0,0,0,0];

function fillCanvas(number)
{
    let canvas = document.getElementById('image');
    let ctx = canvas.getContext('2d');

    for(var i=0;i<imageWidth;i++)
    {
        for(var j=0;j<imageHeight;j++)
        {
            let color = 255*+pixelData[number][j*imageWidth+i];
            ctx.fillStyle = 'rgb('+color+','+color+','+color+')'
            ctx.fillRect(i*pixelSize, j*pixelSize, pixelSize, pixelSize);
        }
    }
}