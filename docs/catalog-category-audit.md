# Catalog Category Audit (STEP 10.5)

Evidence: `Requirements/ExampleDatas/CategoryQueryExample.txt` + `ProductQueryExample.txt`.
Product seed CategoryId numbers disagreed with Category insert order; remapped by dish semantics to Category **names**.

| ProductId | ProductName | CurrentCategory | CanonicalCategory | Evidence | Confidence |
|---|---|---|---|---|---|
| 43 | Izgara Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 44 | Kremalı Mantarlı Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 45 | Fırın Somon | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 46 | Dana Bonfile | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 47 | Et Sote | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 48 | Sebzeli Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 49 | BBQ Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 50 | Kuzu Tandır | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 51 | Dana Antrikot | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 52 | Tavuk Schnitzel | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 53 | Tavuk Fajita | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 54 | Et Fajita | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 55 | Sebzeli Noodle | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 56 | Fırın Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 57 | Mantarlı Bonfile | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 58 | Kremalı Somon | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 59 | Sebzeli Bonfile | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 60 | Teriyaki Tavuk | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 61 | Kuzu Pirzola | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 62 | Izgara Köfte | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 63 | Şefin Tabağı | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 64 | Kuzu Şiş | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 65 | Tavuk Teriyaki | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 66 | Izgara Hindi | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 67 | Sebzeli Tavuk Izgara | Ana Yemekler | Ana Yemekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 1 | Domatesli Bruschetta | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 2 | Karides Tempura | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 3 | Avokado Salsa | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 4 | Fırın Patates Kabuğu | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 5 | Mozzarella Stick | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 6 | Çıtır Soğan Halkası | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 7 | Humus Tabağı | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 8 | Patlıcan Ezmesi | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 9 | Sigara Böreği | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 10 | Nachos Tabağı | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 11 | Karides Kokteyl | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 12 | Mini Burger | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 13 | Zeytinyağlı Enginar | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 14 | Peynir Tabağı | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 15 | Garlic Bread | Başlangıçlar | Başlangıçlar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 16 | Mercimek Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 17 | Ezogelin Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 18 | Domates Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 19 | Tavuk Suyu Çorba | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 20 | Mantar Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 21 | Sebze Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 22 | Tarhana Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 23 | Yoğurt Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 24 | Brokoli Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 25 | Balık Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 26 | Kremalı Tavuk Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 27 | Düğün Çorbası | Çorbalar | Çorbalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 130 | Kola | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 131 | Fanta | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 132 | Sprite | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 133 | Limonata | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 134 | Ice Tea Şeftali | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 135 | Portakal Suyu | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 136 | Elma Suyu | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 137 | Soda | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 138 | Gazoz | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 139 | Ayran | İçecekler | İçecekler | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 68 | Adana Kebap | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 69 | Urfa Kebap | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 70 | Tavuk Şiş | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 71 | Kuzu Şiş | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 72 | Izgara Köfte | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 73 | Dana Şiş | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 74 | Kuzu Pirzola Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 75 | Tavuk Kanat | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 76 | Karışık Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 77 | Antrikot Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 78 | Bonfile Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 79 | Kuzu Kaburga | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 80 | Izgara Sucuk | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 81 | Izgara Tavuk Fileto | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 82 | Köfte Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 83 | Et Şiş Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 84 | Izgara Sebze | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 85 | Kuzu Küşleme | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 86 | Dana Kaburga Izgara | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 87 | Izgara Tavuk Kanat | Izgara & Et Yemekleri | Izgara & Et Yemekleri | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 140 | Espresso | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 141 | Americano | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 142 | Latte | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 143 | Cappuccino | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 144 | Mocha | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 145 | Türk Kahvesi | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 146 | Filtre Kahve | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 147 | Çay | Kahve & Çay | Kahve & Çay | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 88 | Spaghetti Bolognese | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 89 | Fettuccine Alfredo | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 90 | Penne Arrabbiata | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 91 | Spaghetti Carbonara | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 92 | Lasagna | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 93 | Tagliatelle | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 94 | Ravioli | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 95 | Pesto Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 96 | Deniz Mahsullü Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 97 | Kremalı Mantarlı Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 98 | Tavuklu Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 99 | Sebzeli Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 100 | Karidesli Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 101 | Napoli Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 102 | Trüf Mantarlı Pasta | Makarnalar | Makarnalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 103 | Pizza Margherita | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 104 | Pepperoni Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 105 | Quattro Formaggi | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 106 | Vegetarian Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 107 | BBQ Chicken Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 108 | Ton Balıklı Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 109 | Karides Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 110 | Mantarlı Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 111 | Sucuklu Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 112 | Karışık Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 113 | Napoli Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 114 | Prosciutto Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 115 | Mozzarella Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 116 | Akdeniz Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 117 | Sebzeli Pizza | Pizza | Pizza | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 28 | Sezar Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 29 | Akdeniz Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 30 | Tavuklu Yeşil Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 31 | Ton Balıklı Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 32 | Avokado Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 33 | Kinoa Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 34 | Roka Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 35 | Izgara Peynir Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 36 | Karides Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 37 | Fit Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 38 | Sebzeli Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 39 | Nar Ekşili Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 40 | Avokadolu Tavuk Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 41 | Caprese Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 42 | Sebze Bahçesi Salata | Salatalar | Salatalar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 118 | Tiramisu | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 119 | Cheesecake | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 120 | Çikolatalı Sufle | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 121 | Panna Cotta | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 122 | San Sebastian Cheesecake | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 123 | Profiterol | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 124 | Dondurma Tabağı | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 125 | Creme Brulee | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 126 | Brownie | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 127 | Meyve Tabağı | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 128 | Magnolia | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
| 129 | Fırın Sütlaç | Tatlılar | Tatlılar | ProductQueryExample.txt dish block + CategoryQueryExample.txt name | AUTHORITATIVE |
