# SBES-PROJEKAT17

Projektni zadatak iz predmeta "Sigurnost i bezbednost u elektroenergetskim sistemima" na FTN-u u Novom Sadu. 

Zadatak podrazumeva izradu bezbednosnih mehanizama pri izradi sistema za upravljanje servisima.

Komunikacija unutar celog sistema je zastićena uz pomoć AES enkripcije. Prilikom uspostavljanja komunikacije između servera i klijenata vrši se Windows autentifikacija, dok je autorizacija zasnovana na RBAC modelu.

Samo autorizovani korisnici mogu pokretati servise i vršiti izmene.

U okviru projekta postoji blacklist konfiguracija gde se beleže servisi koji su bili izloženi DoS napadu. Takođe, u koliko dođe do DoS napada, SM komponenta prijavljuje događaj Audit komponenti. U slučaju neautorizovane izmene konfiguracionog fajla, SM komponenta prijavljuje događaj Audit komponenti i dolazi do zaustavljanja rada servisa.

Posebna grupa klijenata ima pravo da menja blacklist konfiguraciju, odnosno periodično proverava da li je narušen integritet fajla gde se konfiguracija skladišti.
