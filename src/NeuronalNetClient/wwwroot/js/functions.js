var canvas;
var ctx;

function init()
{
    canvas = document.getElementById('image');
    ctx = canvas.getContext('2d');

    document.getElementById('files').addEventListener('change', dateiauswahl, false);

    while(true)
    {
        var img = document.getElementById('list');
        canvas.width = img.width;
        canvas.height = img.height;
        ctx.drawImage(img, 0, 0 );
    }
}

function fillCanvas(number)
{
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
