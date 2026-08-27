# Rakennuspeli

Tämä peli on luotu esimerkkiprojektiksi Pelikehittäjänä toimiminen -kurssille. Siinä on käytetty erilaisia olio-ohjelmoinnin tekniikoita:
- Rajapinnat
- Periytyminen
- Komentojen tallentaminen olioina

## Miten peli toimii?

Pelin tarkoituksena on rakentaa resursseista ja rakennuksista koostuvia tuotantoketjuja ja rakentaa aina vain lisää rakennuksia.

Pelin maailmassa on **resursseja**. Tietyt rakennukset ottavat resurssia talteen. Esimerkiksi **Water Extractor** ottaa vettä ja **Solar Panel** tuottaa sähköä.
Rakennukset voivat tuottaa ja kuluttaa resursseja. Jokaisella rakennuksella on määritelty mitä se tarvitsee ja mitä se tuottaa.

Resursseja voi liikuttaa ympäriinsä käyttäen ajoneuvoja. Esim **Water Truck** kuljettaa vettä. Ajoneuvoille annetaan ohjeita joita ne noudattavat.

Pelaaja voi rakentaa lisää rakennuksia ja ajoneuvoja.

## Rakennukset, ajoneuvot ja resurssit

Pelissä on valmiina 5 erilaista rakennusta:
- Water Extractor : Kerää vettä Lake -resurssista
- Solar Panel : Tuottaa sähköä
- Power Transmitter : Siirtää sähköä rakennusten välillä
- Depot : Rakentaa ajoneuvoja
- Factory : Tarvitsee resursseja ja tuottaa metallia

Pelissä on valmiina yksi ajoneuvo:
- Water Truck : Kuljettaa vettä

Pelissä on valmiina yksi resurssi:
- Lake : tästä resurssista saadaan vettä

Lisäksi on määritelty resurssit Ore, Power ja Metal.

## Kontrollit

- Kameraa voi liikuttaa WASD napeilla tai nuolilla.
- Kameraa voi zoomata hiiren rullalla.
- Rakennuksen tai ajoneuvon voi valita hiiren vasemmalla napilla
- Rakennuksen tai ajoneuvon voi rakentaa klikkaamalla sen kuvaa vasemmalla napilla. Tämän jälkeen peli menee rakentamistilaan jossa rakennuksen tai ajoneuvon voi sijoittaa kartalle painamalla vasenta nappia.
- Rakentamisen voi perua tai valinnan tyhjentää hiiren oikealla napilla.

### Ajoneuvojen ohjeet

Ajoneuvolle voi antaa ohjeita valitsemalla ensin ajoneuvon. Rakennuksia klikataan siinä järjestyksessä kuin haluaa ajoneuvon käyvän niissä. Jos valitsee rakennuksen joka ei tuota tai vastaanota ajoneuvon kuljettamaa resurssia, ajoneuvo jää jumiin.

Annetut ohjeet voi tyhjentää painamalla valikon nappia.
