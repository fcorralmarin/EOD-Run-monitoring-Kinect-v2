# Installation
## 1. Install Kinect dependencies
* [Kinect Runtime 2.0](https://www.microsoft.com/en-us/download/details.aspx?id=44559)
* [Kinect SDK 2.0](https://www.microsoft.com/en-us/download/details.aspx?id=44561)

## 2. Usage

### 2.1 Client App:
Fill "Server IP" textbox wit server IP shown in Server app and press "Connect" button, if it has been already introduced a valid IP it will be autofilled. Status bar will be showing application status and Kinect device current FPS.

<figure>
  <img src="Images/ClientDisc.png" alt="x"/>
  <figcaption>Before connecting.</figcaption>
</figure>

<figure>
  <img src="Images/ClientCon.png" alt="x"/>
  <figcaption>After connecting.</figcaption>
</figure>

### 2.2 Server App:

#### 2.2.1 Server home menu:

Home view of Server application to decide between modules:
* Session
* Data history

<figure>
  <img src="Images/Home.png" alt="x"/>
  <figcaption>Home menu.</figcaption>
</figure>

#### 2.2.2 Server session recording:

Session view to record session after connecting with Client application.

<figure>
  <img src="Images/ServerDisc.png" alt="x"/>
  <figcaption>Before connecting. IP list.</figcaption>
</figure>

<figure>
  <img src="Images/ServerPrepare.png" alt="x"/>
  <figcaption>Preparing before session.</figcaption>
</figure>

<figure>
  <img src="Images/Session_Nostop.png" alt="x"/>
  <figcaption>Before 30s session.</figcaption>
</figure>

<figure>
  <img src="Images/Session_stop.png" alt="x"/>
  <figcaption>After 30s session.</figcaption>
</figure>

#### 2.2.3 Server session saving:

Save session view to save recorded sessions

<figure>
  <img src="Images/SaveSession2.png" alt="x"/>
  <figcaption>Searching for an athlete id that is not present.</figcaption>
</figure>

<figure>
  <img src="Images/SaveSession3.png" alt="x"/>
  <figcaption>Searching for an athlete id that is present.</figcaption>
</figure>

<figure>
  <img src="Images/SaveSession4.png" alt="x"/>
  <figcaption>Filling missing information before being able to save.</figcaption>
</figure>

#### 2.2.4 Server data history:

Data history contains all recorded sessions and can be filtered by:
* Complete match of ID
* Full or beggining of name
* Gender (Man/Woman/Both)
* From date
* To date

Selected sessions can be:
* Deleted (Single/Multi select)
* Played (Single select)
<figure>
  <img src="Images/DataHistory1.png" alt="x"/>
  <figcaption>Full data history view.</figcaption>
</figure>

<figure>
  <img src="Images/DataHistory2.png" alt="x"/>
  <figcaption>Searching incomplete/invalid id yields not results.</figcaption>
</figure>

<figure>
  <img src="Images/DataHistory3.png" alt="x"/>
  <figcaption>Searching complete/valid id yields results.</figcaption>
</figure>

<figure>
  <img src="Images/DataHistory4.png" alt="x"/>
  <figcaption>Searching by name or beggining of name.</figcaption>
</figure>

<figure>
  <img src="Images/DataHistory5.png" alt="x"/>
  <figcaption>Filtering by From date.</figcaption>
</figure>

<figure>
  <img src="Images/DataHistory6.png" alt="x"/>
  <figcaption>Filtering by From date.</figcaption>
</figure>


#### 2.2.5 Server session re play:


<figure>
  <img src="Images/PlaySession.png" alt="x"/>
  <figcaption>Playing an stored session.</figcaption>
</figure>