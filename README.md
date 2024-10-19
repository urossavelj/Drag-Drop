# Drag&Drop

To start the app, build the solution first (both API and UI). Than set UI as startup project and start it.

On the left hand size you have some labels, which you can drag & drop to the canvas on the right. When you do that, they become textboxes.

![image](https://github.com/user-attachments/assets/f6deec07-4adc-4745-be6c-42798c8ae6eb)

If you click save (on the button on the right), the canvas settings will be saved in a json file called positions.json (Drag&Drop\¸UI\bin\Debug\net8.0-windows), and the server settings (if the API is running) will be saved in the SQLite database. The database is stored in a settings.db file (in my case in C:\Users\uross\AppData\Local).

On the right side you also have buttons Send, Start and Stop.

- button Start starts the API (default is address https://localhost (/Settings) and port 7188)
- button Stop kills the API service
- button Send will send a request to the API, with the url and port set in the Client outbound address and outbound port. It will send also the Body and HTTP parameters set on the right side panel. HTTP params ex.: param1, value1; param2, value2;
- button Save as written before also updates the API server address and port (if filled out) and if API server is running (but the outbound address and port has to be correct - the current address and port). If you update it, than you can stop the service and press start again and the API server will run on the new address.

 
