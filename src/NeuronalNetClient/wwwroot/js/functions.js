function init()
{
    document.getElementById('files').addEventListener('change', dateiauswahl, false);
}

let imageWidth = 7;
let imageHeight = 7;
let pixelSize = 40;

var pixelData = [];
pixelData[0] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,1,0,0,0, 0,1,0,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[1] = [0,0,0,0,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[2] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,1,1,1,0,0,0, 0,1,0,0,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[3] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[4] = [0,0,0,0,0,0,0, 0,1,0,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[5] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,0,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[6] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[7] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[8] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];
pixelData[9] = [0,0,0,0,0,0,0, 0,1,1,1,0,0,0, 0,1,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,1,0,0,0, 0,1,1,1,0,0,0, 0,0,0,0,0,0,0];

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
            //ctx.fillRect(i*pixelSize, j*pixelSize, pixelSize, pixelSize);
        }
    }

    var img = document.getElementById('list');
    canvas.width = img.width;
    canvas.height = img.height;
    ctx.drawImage(img, 0, 0 );
    var myData = ctx.getImageData(0, 0, img.width, img.height);
    var allData = [];
    allData.push(img.width);
    allData.push(number);
    for (var i=0;i<myData.data.length;i+=4)
    {
        allData.push(myData.data[i]);
        allData.push(myData.data[i+1]);
        allData.push(myData.data[i+2]);
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
        };
        })(f);
        // Bilder als Data URL auslesen.
        reader.readAsDataURL(f);
    }
}
