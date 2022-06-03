class Vector2D {
  constructor(x=0, y=0) {
      this.x = x;
      this.y = y;
    }
}

class Box extends Vector2D {
  constructor(x=0,y=0,width=0, height=0) {
    super(x,y);
    this.width = width;
    this.height = height;
  }
}

class Pixel extends Box {
  constructor(x=0,y=0,width=0,height=0,red=0,green=0,blue=0) {
    super(x,y,width,height);
    this.red = red;
    this.green = green;
    this.blue = blue;
  }

  setColor(red,blue=red,green=red) {
    this.red = red;
    this.green = green;
    this.blue = blue;
  }

  mouseHold() {
    if(MOUSE_DOWN) {
      let x1 = MOUSE_CURSOR_POSITION.x;
      let y1 = MOUSE_CURSOR_POSITION.y;
      let x2 = LAST_MOUSE_CURSOR_POSITION.x;
      let y2 = LAST_MOUSE_CURSOR_POSITION.y;

      let minX = this.x-2;
      let minY = this.y-2;
      let maxX = this.x+this.width+2;
      let maxY = this.y+this.height+2;

      // Completely outside.
      if ((x1 <= minX && x2 <= minX) || (y1 <= minY && y2 <= minY) || (x1 >= maxX && x2 >= maxX) || (y1 >= maxY && y2 >= maxY))
          return false;

      var m = (y2 - y1) / (x2 - x1);

      var y = m * (minX - x1) + y1;
      if (y > minY && y < maxY) return true;

      y = m * (maxX - x1) + y1;
      if (y > minY && y < maxY) return true;

      var x = (minY - y1) / m + x1;
      if (x > minX && x < maxX) return true;

      x = (maxY - y1) / m + x1;
      if (x > minX && x < maxX) return true;

      return false;
    }
    return false;
  }

  mouseClick() {
    if(MOUSE_CLICK&&MOUSE_CURSOR_POSITION.x>=this.x&&MOUSE_CURSOR_POSITION.x<this.x+this.width&&MOUSE_CURSOR_POSITION.y>=this.y&&MOUSE_CURSOR_POSITION.y<this.y+this.height)
      return true;
    return false;
  }

  draw() {
    setColor(this.red*255,this.green*255,this.blue*255);
    drawRect(this.x,this.y, this.width,this.height);
  }
}

class ClickImage {
  constructor(x,y,width,height,pixelSize) {
    this.pixels = [];
    for(let i=0;i<width;i++) {
      for(let j=0;j<height;j++) {
        this.pixels.push(new Pixel(x+i*pixelSize,y+j*pixelSize,pixelSize,pixelSize));
      }
    }
    OBJECT_LIST.push(this);
  }

  update() {
    for(let i=0;i<this.pixels.length;i++) {
      if(this.pixels[i].mouseClick()||this.pixels[i].mouseHold()) {
        this.pixels[i].setColor(1);
      }
    }
  }

  reset() {
    for(let i=0;i<this.pixels.length;i++) {
      this.pixels[i].setColor(0);
    }
  }

  draw() {
    for(let i=0;i<this.pixels.length;i++) {
      this.pixels[i].draw();
    }
  }
}

class InputLayer {
  constructor(image) {
    this.image = image;
    this.size = this.image.pixels.length;
    this.values = [];

    this.d_values = [];
    this.d_values_samples = [];

    for(let i=0;i<this.size;i++) {
      this.d_values[i] = 0;
      this.d_values_samples[i] = 0;
    }

  }

  update() {
    for(let i=0;i<this.image.pixels.length;i++) {
      this.values[i] = this.image.pixels[i].red;
    }
  }

  draw(x,y,size) {
    for(let i=0;i<this.values.length;i++) {
      setColor(this.values[i]*255,this.values[i]*255,this.values[i]*255);
      drawRect(new Vector2D(x,y+i*size), new Vector2D(size,size));
    }
  }
}

class NeuralLayer {
  constructor(size, previousLayer) {
    this.size = size;
    this.previousLayer = previousLayer;

    this.functions = [];
    this.values = [];
    this.weights = [];
    this.biases = [];

    this.d_values = [];
    this.d_weights = [];
    this.d_biases = [];

    this.d_values_samples = [];
    this.d_weights_samples = [];
    this.d_biases_samples = [];

    for(let i=0;i<this.size;i++) {
      this.biases[i] = Math.random()*2-1;
      this.d_values[i] = 0;
      this.d_values_samples[i] = 0;
      this.d_biases[i] = 0;
      this.d_biases_samples[i] = 0;
      for(let j=0;j<this.previousLayer.size;j++) {
        this.weights[i+j*this.size] = Math.random()*2-1;
        this.d_weights[i+j*this.size] = 0;
        this.d_weights_samples[i+j*this.size] = 0;
      }
    }
  }

  backpropagate() {
    for(let i=0;i<this.size;i++) {
      this.d_biases[i] += 1*d_sig(this.functions[i])*this.d_values[i]/this.d_values_samples[i];
      this.d_biases_samples[i]++;
      for(let j=0;j<this.previousLayer.size;j++) {
          this.d_weights[i+j*this.size] += this.previousLayer.values[j]*d_sig(this.functions[i])*this.d_values[i]/this.d_values_samples[i];
          this.d_weights_samples[i+j*this.size]++;
          this.previousLayer.d_values[j] += this.weights[i+j*this.size]*d_sig(this.functions[i])*this.d_values[i]/this.d_values_samples[i];
          this.previousLayer.d_values_samples[j]++;
      }
    }

    for(let i=0;i<this.size;i++) {
        this.d_values_samples[i] = 0;
    }
  }

  improveNet() {
    for(let i=0;i<this.size;i++) {
      this.biases[i] -= this.d_biases[i]/this.d_biases_samples[i]*0.1;//*learnRate;//;
      this.d_biases[i] = 0;
      this.d_biases_samples[i] = 0;
      for(let j=0;j<this.previousLayer.size;j++) {
          this.weights[i+j*this.size] -= this.d_weights[i+j*this.size]/this.d_weights_samples[i+j*this.size]*0.1;//*learnRate;//;
          this.d_weights[i+j*this.size] = 0;
          this.d_weights_samples[i+j*this.size] = 0;;
          this.previousLayer.d_values[j] = 0;
          this.previousLayer.d_values_samples[j] = 0;
      }
    }
  }

  update() {
    for(let i=0;i<this.size;i++) {
      this.functions[i] = this.biases[i];
      for(let j=0;j<this.previousLayer.size;j++) {
        this.functions[i] += this.weights[i+j*this.size]*this.previousLayer.values[j];
      }
      this.values[i] = sig(this.functions[i]);
    }
  }

  copyState(layer) {
    for(let i=0;i<this.size;i++) {
      this.biases[i] = layer.biases[i];
      for(let j=0;j<this.previousLayer.size;j++) {
          this.weights[i+j*this.size] = layer.weights[i+j*this.size];
      }
    }
  }

  evalCost(value) {
    var cost = 0;
    for(let i=0;i<this.values.length;i++) {
      this.d_values_samples[i] = 1;
      if(i==value) {
        cost += Math.pow(1-this.values[i],2);
        this.d_values[i] = 2*(this.values[i]-1);
      }
      else {
        cost += Math.pow(this.values[i],2);
        this.d_values[i] = 2*this.values[i];
      }
    }
    return cost;
  }

  getMax() {
    var max = 0;
    var maxValue = 0;
    for(let i=0;i<this.values.length;i++) {
      if(this.values[i]>maxValue) {
        max = i;
        maxValue = this.values[i];
      }
    }
    return max;
  }

  draw(x,y,size) {
    for(let i=0;i<this.values.length;i++) {
      setColor(this.values[i]*255,this.values[i]*255,this.values[i]*255);
      drawRect(new Vector2D(x,y+i*size), new Vector2D(size,size));
    }
  }
}

class NeuralNet {
  constructor(input, layerCount, layerSize, outputSize) {
    this.layerCount = layerCount;
    this.layerSize = layerSize;
    this.inputLayer = new InputLayer(input);
    this.layers = [new NeuralLayer(this.layerSize,this.inputLayer)];
    for(let i=1;i<this.layerCount;i++) {
      this.layers.push(new NeuralLayer(this.layerSize,this.layers[i-1]));
    }
    this.outputLayer = new NeuralLayer(outputSize,this.layers[this.layerCount-1]);
  }

  update() {
    this.inputLayer.update();
    for(let i=0;i<this.layerCount;i++) {
      this.layers[i].update();
    }
    this.outputLayer.update();
  }

  copyState(net) {
    for(let i=0;i<this.layerCount;i++) {
      this.layers[i].copyState(net.layers[i]);
    }
    this.outputLayer.copyState(net.outputLayer);
  }

  evalCost(value) {
    return this.outputLayer.evalCost(value);
  }

  backpropagate() {
    this.outputLayer.backpropagate();
    for(let i=this.layerCount-1;i<=0;i--) {
      if(i<0)
        break;
      this.layers[i].backpropagate();
    }
  }

  improveNet() {
    this.outputLayer.improveNet();
    for(let i=this.layerCount-1;i<=0;i--) {
      if(i<0)
        break;
      this.layers[i].improveNet();
    }
  }

  draw(x,y,size) {
    this.inputLayer.draw(x,y,size.y/this.inputLayer.size);
    for(let i=0;i<this.layerCount;i++) {
      this.layers[i].draw(x+(i+1)*size.x/(this.layerCount+2),y,size.y/this.layerSize);
    }
    this.outputLayer.draw(x+(this.layerCount+1)*size.x/(this.layerCount+2),y,size.y/this.outputLayer.size);
  }
}

class Slider {
  constructor(x,y,width,height) {
    this.pixel = new Pixel(x,y,width,height);
    this.slide = new Pixel(x,y,height,height);
    this.slide.setColor(0.3);
    this.value = 0;
    OBJECT_LIST.push(this);
  }

  update() {
    if(this.pixel.mouseHold()) {
      this.slide.x = Math.min(Math.max(MOUSE_CURSOR_POSITION.x-this.slide.width/2,this.pixel.x),this.pixel.x+this.pixel.width-this.slide.width);
      this.value = (this.slide.x-this.pixel.x)/(this.pixel.width-this.slide.width);
    }
  }

  draw() {
    this.pixel.draw();
    this.slide.draw();
  }

}

class TextField {
  constructor(x,y,width,height,text) {
    this.pixel = new Pixel(x,y,width,height);
    this.text = text;
    OBJECT_LIST.push(this);
  }

  mouseClick() {
    if(this.pixel.mouseClick())
      return true;
    return false;
  }

  update() {
    if(this.pixel.mouseClick()) {
      this.pixel.setColor(0.5);
    }
    else
      this.pixel.setColor(0);
  }

  draw() {
    this.pixel.draw();
    setColor(155,155,155);
    drawText(this.text,this.pixel.x+15,this.pixel.y+this.pixel.height-10);
  }
}

var canvas = document.getElementById('canvas');
var ctx = canvas.getContext('2d');

var learnRate = 1;

var LAST_MOUSE_CURSOR_POSITION = new Vector2D(0,0);
var MOUSE_CURSOR_POSITION = new Vector2D(0,0);
var MOUSE_CLICK = false;
var MOUSE_DOWN = false;

var STEPS = 100;
var STEPS_PER_TICK = 2;

var OBJECT_LIST = [];

getFileFromServer("data.txt", getTextFromServer);

var currentCost = 0;

var lastRender = 0
window.requestAnimationFrame(loop);

var imageNumber = new ClickImage(10,10,64,64,2);
var improveImage = new ClickImage(510,10,64,64,2);

var button0 = new TextField(200,10,40,30,"0");
var button1 = new TextField(250,10,40,30,"1");
var button2 = new TextField(300,10,40,30,"2");
var button3 = new TextField(350,10,40,30,"3");
var button4 = new TextField(400,10,40,30,"4");
var button5 = new TextField(200,60,40,30,"5");
var button6 = new TextField(250,60,40,30,"6");
var button7 = new TextField(300,60,40,30,"7");
var button8 = new TextField(350,60,40,30,"8");
var button9 = new TextField(400,60,40,30,"9");
var buttonR = new TextField(200,210,80,30,"reset");
var buttonG = new TextField(200,110,110,30,"Guess ");
var buttonC = new TextField(330,110,110,30,"Cost " + currentCost);
var buttonH = new TextField(200,160,240,30,"Hitrate " + 0 + " / " + 0 + " : " + 0 + "%");

var buttonI = new TextField(300,210,140,30,"Get Data");

var slider = new Slider(510,160,128,30);

var netData = "";
var hits = [];
for(let i=0;i<STEPS;i++)
  hits[i] = 0;
var trials = 0;
var generalCost = [];
for(let i=0;i<STEPS;i++)
  generalCost[i] = 10;

var c = 0;

var neuralNet = new NeuralNet(imageNumber,2,32,10);
var improveNeuralNet = new NeuralNet(improveImage,2,32,10);


function getFileFromServer(url, doneCallback) {
    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = handleStateChange;
    xhr.open("GET", url, true);
    xhr.send();

    function handleStateChange() {
        if (xhr.readyState === 4) {
            doneCallback(xhr.status == 200 ? xhr.responseText : null);
        }
    }
}

function getHits() {
  var sum = 0;
  for(let i=0;i<hits.length;i++)
    sum += hits[i];
  return sum;
}

function getCost() {
  var sum = 0;
  for(let i=0;i<generalCost.length;i++)
    sum += generalCost[i];
  return sum;
}

function getTextFromServer(text) {
  netData = text;
}

function loop(timestamp) {
  var progress = timestamp - lastRender;

  resizeCanvas();
  draw();

  for(let i=0;i<OBJECT_LIST.length;i++) {
    OBJECT_LIST[i].update();
    OBJECT_LIST[i].draw();
  }

  if(button0.mouseClick())
    calculateNeuralNetAndSave(0,neuralNet,imageNumber);
  if(button1.mouseClick())
    calculateNeuralNetAndSave(1,neuralNet,imageNumber);
  if(button2.mouseClick())
    calculateNeuralNetAndSave(2,neuralNet,imageNumber);
  if(button3.mouseClick())
    calculateNeuralNetAndSave(3,neuralNet,imageNumber);
  if(button4.mouseClick())
    calculateNeuralNetAndSave(4,neuralNet,imageNumber);
  if(button5.mouseClick())
    calculateNeuralNetAndSave(5,neuralNet,imageNumber);
  if(button6.mouseClick())
    calculateNeuralNetAndSave(6,neuralNet,imageNumber);
  if(button7.mouseClick())
    calculateNeuralNetAndSave(7,neuralNet,imageNumber);
  if(button8.mouseClick())
    calculateNeuralNetAndSave(8,neuralNet,imageNumber);
  if(button9.mouseClick())
    calculateNeuralNetAndSave(9,neuralNet,imageNumber);

  c++;
  if(buttonI.mouseClick()) {
    console.log(netData)
    //simulateAllData(improveNeuralNet,improveImage);
    //neuralNet.copyState(improveNeuralNet);
  }
  if(slider.value>1||Math.floor(c%(1/(slider.value)))==0) {
    c = 0;
    simulateRandomData(improveNeuralNet,improveImage);
  }
  neuralNet.copyState(improveNeuralNet);

  buttonG.text = "Guess " + neuralNet.outputLayer.getMax();
  buttonC.text = "Cost " + Math.floor(currentCost*100)/100;


  if(buttonR.mouseClick())
    imageNumber.reset();

  neuralNet.update();
  neuralNet.draw(10,180,new Vector2D(175,175));

  MOUSE_CLICK = false;
  LAST_MOUSE_CURSOR_POSITION = MOUSE_CURSOR_POSITION;

  lastRender = timestamp;
  window.requestAnimationFrame(loop);
}

document.addEventListener("mousemove", getMouseCoords);
document.addEventListener("mousedown", mouseDown);
document.addEventListener("mouseup", mouseRelease);

function simulateAllData(net,image) {
  var splittedNetData = netData.split('\n');
  for(let i=0;i<splittedNetData.length-1;i++) {
    var realValue = splittedNetData[i][0];
    var binaryNetData = splittedNetData[i].substr(1,64*64);
    for(let j=0;j<binaryNetData.length;j++) {
      image.pixels[j].setColor(binaryNetData[j]);
    }
    net.update();
    calculateNeuralNet(realValue,net,image);
  }
  net.improveNet();
}

function simulateRandomData(net,image) {
  var splittedNetData = netData.split('\n');
  let randData = Math.floor(Math.random()*(splittedNetData.length-1));
  var realValue = splittedNetData[randData][0];
  var binaryNetData = splittedNetData[randData].substr(1,64*64);
  for(let j=0;j<binaryNetData.length;j++) {
    image.pixels[j].setColor(binaryNetData[j]);
  }
  net.update();
  calculateNeuralNet(realValue,net,image);
  net.improveNet();

  if(trials==STEPS) {
    buttonH.text = "Hitrate " + getHits() + " / " + STEPS  + " : " + Math.floor(getCost()*100)/(100*STEPS);
    trials = 0;
  }
}

function getMouseCoords(event) {
  MOUSE_CURSOR_POSITION = new Vector2D(event.clientX,event.clientY);
}

function mouseDown(event) {
  MOUSE_CLICK = true;
  MOUSE_DOWN = true;
}

function mouseRelease(event) {
  MOUSE_DOWN = false;
  MOUSE_CLICK = false;
}

function resizeCanvas() {
  canvas.width = window.innerWidth;
  canvas.height = window.innerHeight;
}

function calculateNeuralNetAndSave(value,net,image) {
  saveNetData(value);
  currentCost = net.evalCost(value);
  net.backpropagate();
  //net.improveNet();
  image.reset();
}

function calculateNeuralNet(value,net,image) {
  if(value==net.outputLayer.getMax())
    hits[trials] = 1;
  else
    hits[trials] = 0;
  generalCost[trials] = net.evalCost(value);
  net.backpropagate();
  trials++;

  //image.reset();
}

function saveNetData(value) {
  var input = "";
  for(let i=0;i<neuralNet.inputLayer.values.length;i++) {
    input += neuralNet.inputLayer.values[i];
  }

  netData += value;
  netData += input;
  netData += "\n";
}

function sig(x) {
  return 1.0/(1.0+Math.exp(-x));
}

function d_sig(x) {
  return sig(x)*(1.0-sig(x));
}

function draw() {
  setColor(0,20,120);
  if(MOUSE_DOWN)
    drawRect(MOUSE_CURSOR_POSITION,new Vector2D(10));
  //ctx.fillRect(0, 0, canvas.width, canvas.height);
}

function setColor(r,g,b) {
  ctx.fillStyle = "rgb("+r+","+g+","+b+")";
}

function drawText(text,x,y) {
  ctx.font = "20px Arial";
  ctx.fillText(text, x, y);
}

function drawRect(a,b,c=null,d=null) {
  if(c==null)
    ctx.fillRect(a.x, a.y, b.x, b.y);
  else if(d==null)
    ctx.fillRect(a.x, a.y, b, c);
  else
    ctx.fillRect(a, b, c, d);
}
