# **photovoltaicData**

This is short data collector for Solar panel inverter **model SF4ES010**
I had no idea what and how they push their data and to which service, so decided to create my own collector based on Azure Functions.

## **Functions overview**

Project contains two Azure Functions: ShowData and collectData both written in C#

### **collectData** function

Bindings:
* Blob to append data *CloudAppendBlob* type
* Time trigger for function 

Requires settings:
* URL : http://<<INVENTERSERVICE>>:<<PORT>>/status.html
* PhotoPass: username:password

It appends to blob new line with json object containing:
* current time (CET format),
* current Wattage
* total today kWh
* total kWh

### **ShowData**
Bindings:
* Blob to read data *CloudAppendBlob* type
* Blob to read template.html *CloudBlockBlob* type
* HttpTrigger

It uses template in html, replace some marked ##VALUES### and returns html *ContentResult*

![ShowData gauge](/images/screen.png)






