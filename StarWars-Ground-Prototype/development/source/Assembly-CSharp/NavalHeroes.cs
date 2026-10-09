using System.Collections.Generic;

public class NavalHeroes
{
	public static List<string> GetHeroes(string p_country)
	{
		List<string> list = new List<string>();
		switch (p_country)
		{
		case "Australia":
			list.Add("Tyler Anderson");
			list.Add("Logan White");
			list.Add("Flynn Lewis");
			list.Add("Aiden Hall");
			list.Add("Elijah Campbell");
			list.Add("Oliver Hughes");
			list.Add("Henry Clark");
			list.Add("Darcy Baker");
			list.Add("Charlie King");
			list.Add("Samuel Green");
			break;
		case "Belgium":
			list.Add("Matthias Wouters");
			list.Add("Benjamin Desmet");
			list.Add("Elias Vermeulen");
			list.Add("Jules Verhoeven");
			list.Add("Nicolas Devos");
			list.Add("Oscar Maes");
			list.Add("Tristan Verschueren");
			list.Add("Samuel Cools");
			list.Add("Axel Coppens");
			list.Add("Louis Vandenbergh");
			break;
		case "Bulgaria":
			list.Add("Borislav Dimitrov");
			list.Add("Plamen Popov");
			list.Add("Bogdan Petrov");
			list.Add("Yavor Stoyanov");
			list.Add("Kiril Georgiev");
			list.Add("Stanimir Ivanov");
			list.Add("Radostin Hristov");
			list.Add("Asen Velikov");
			list.Add("Valentin Kolev");
			list.Add("Lyuben Angelov");
			break;
		case "Canada":
			list.Add("Colton Blanchard");
			list.Add("Ethan Lefebvre");
			list.Add("Austin Roy");
			list.Add("Jackson Simard");
			list.Add("Cameron Boudreau");
			list.Add("Noah Côté");
			list.Add("Justin Pelletier");
			list.Add("Aiden Ouellet");
			list.Add("Wyatt Bernard");
			list.Add("Brayden Poirier");
			break;
		case "Croatia":
			list.Add("Josip Pavić");
			list.Add("Lovro Vuković");
			list.Add("Matej Radić");
			list.Add("Ivan Matić");
			list.Add("Andrija Novak");
			list.Add("Ante Marinić");
			list.Add("Filip Kovač");
			list.Add("Marin Horvat");
			list.Add("Stjepan Ćosić");
			list.Add("Petar Barić");
			break;
		case "China":
			list.Add("Bin Zhou");
			list.Add("Shi Deng");
			list.Add("Rui Ma");
			list.Add("Chun Wu");
			list.Add("Jie Hu");
			list.Add("Jia Zhao");
			list.Add("Qiang Ren");
			list.Add("Yi Feng");
			list.Add("Xun Li");
			list.Add("Zhen He");
			break;
		case "Denmark":
			list.Add("Simon Knudsen");
			list.Add("Peter Svendsen");
			list.Add("Anders Larsen");
			list.Add("Michael Christensen");
			list.Add("Patrick Jensen");
			list.Add("Markus Rasmussen");
			list.Add("Oliver Pedersen");
			list.Add("Sebastian Nielsen");
			list.Add("Victor Hansen");
			list.Add("Lucas Poulsen");
			break;
		case "Finland":
			list.Add("Pekka Heikkinen");
			list.Add("Joni Koivisto");
			list.Add("Kari Hakala");
			list.Add("Vesa Laine");
			list.Add("Petri Soininen");
			list.Add("Jarmo Niemelä");
			list.Add("Simo Takala");
			list.Add("Henri Salo");
			list.Add("Erkki Kallio");
			list.Add("Aarne Virta");
			break;
		case "France":
			list.Add("Alexandre Caron");
			list.Add("Baptiste Roussel");
			list.Add("Arthur Roy");
			list.Add("Mathéo Vincent");
			list.Add("Lucien Simon");
			list.Add("Thibault Lemoine");
			list.Add("Timothée Marchand");
			list.Add("Clément Dumont");
			list.Add("Emmanuel Noel");
			list.Add("Esteban Faure");
			break;
		case "Greece":
			list.Add("Takis Papandreou");
			list.Add("Andreas Papadopoulos");
			list.Add("Kostas Ioannis");
			list.Add("Nikos Nikolopoulos");
			list.Add("Dimitris Konstantopoulos");
			list.Add("Yiannis Papazoglou");
			list.Add("Panos Sotiriou");
			list.Add("Giorgos Karagiannis");
			list.Add("Vasilis Papadakis");
			list.Add("Thanasis Antonopoulos");
			break;
		case "Great Britain":
			list.Add("John Linton");
			list.Add("Anthony Miers");
			list.Add("Donald Cameron");
			list.Add("Richard Stannard");
			list.Add("James Magennis");
			list.Add("Ethan King");
			list.Add("David Taylor");
			list.Add("Lucas Campbell");
			list.Add("Mason Brown");
			list.Add("Logan Wilson");
			list.Add("Noah Ross");
			list.Add("Finley Phillips");
			list.Add("Aiden Martin");
			list.Add("Cameron Young");
			list.Add("Liam Cooper");
			break;
		case "Hungary":
			list.Add("Bálint Bán");
			list.Add("Dénes Gál");
			list.Add("Gergely Mészáros");
			list.Add("Imre Bíró");
			list.Add("Károly Fodor");
			list.Add("Levente Szűcs");
			list.Add("Miklós Pál");
			list.Add("Ottó Kerekes");
			list.Add("Pálfi Vörös");
			list.Add("Soma Barta");
			break;
		case "Italy":
			list.Add("Inigo Campioni");
			list.Add("Gino Pavesi");
			list.Add("Angelo Parona");
			list.Add("Giovanni Galati");
			list.Add("Gino Birindelli");
			list.Add("Luigi Mascherpa");
			list.Add("Mario De Santis");
			list.Add("Stefano Bianco");
			list.Add("Leonardo Ferrara");
			list.Add("Salvatore Rossetti");
			list.Add("Daniele Greco");
			list.Add("Vincenzo Amato");
			list.Add("Cristiano Russo");
			list.Add("Emanuele Mariani");
			list.Add("Claudio Villa");
			list.Add("Gabriele Farina");
			break;
		case "Japan":
			list.Add("Naokuni Nomura");
			list.Add("Kondo Nobutake");
			list.Add("Takasu Shirō");
			list.Add("Tsukahara Nishizō");
			list.Add("Shiozawa Koichi");
			list.Add("Haruki Nakajima");
			list.Add("Yusuke Kuroda");
			list.Add("Takumi Ito");
			list.Add("Ren Ishikawa");
			list.Add("Kazuki Yamada");
			list.Add("Ryota Sasaki");
			list.Add("Sosuke Okada");
			list.Add("Daiki Nakagawa");
			list.Add("Yuta Aoki");
			break;
		case "Nazi Germany":
			list.Add("Erich Raeder");
			list.Add("Günther Lütjens");
			list.Add("Kurt Fricke");
			list.Add("Theodor Burchardi");
			list.Add("Wilhelm Canaris");
			list.Add("Wilhelm Meisel");
			list.Add("Daniel Schuster");
			list.Add("David Koch");
			list.Add("Patrick Bauer");
			list.Add("Tom Sommer");
			list.Add("Marcel Frank");
			list.Add("Jannik Braun");
			list.Add("Robin Herrmann");
			list.Add("Fabio Walter");
			list.Add("Marco Mayer");
			list.Add("Oliver Brandt");
			break;
		case "Norway":
			list.Add("Johan Jensen");
			list.Add("William Svendsen");
			list.Add("Kristoffer Henriksen");
			list.Add("Theodor Larsen");
			list.Add("Petter Andersen");
			list.Add("Magnus Eide");
			list.Add("Lucas Mathisen");
			list.Add("Lars Strand");
			list.Add("Alexander Berg");
			list.Add("Bjørn Vik");
			break;
		case "New Zealand":
			list.Add("Maxwell Williams");
			list.Add("Harrison Wilson");
			list.Add("Cooper Moore");
			list.Add("Jesse Thomas");
			list.Add("Rowan Thompson");
			list.Add("Owen Jackson");
			list.Add("Dylan Harris");
			list.Add("Hugo Evans");
			list.Add("Patrick White");
			list.Add("Mason Martin");
			break;
		case "Netherlands":
			list.Add("Conrad Helfrich");
			list.Add("Karel Doorman");
			list.Add("Tim de Jong");
			list.Add("Joep Bakker");
			list.Add("Benjamin Koster");
			list.Add("Samuel van der Wal");
			list.Add("Matthijs Maas");
			list.Add("Jasper Pauw");
			list.Add("Dennis de Wit");
			list.Add("Kevin Vos");
			list.Add("Wessel van der Horst");
			list.Add("Rick van Beek");
			break;
		case "Poland":
			list.Add("Adrian Sikorski");
			list.Add("Artur Nowicki");
			list.Add("Hubert Tomaszewski");
			list.Add("Sebastian Pietrzak");
			list.Add("Janusz Zalewski");
			list.Add("Kacper Krawczyk");
			list.Add("Maciej Olejniczak");
			list.Add("Robert Wróbel");
			list.Add("Dariusz Kwiatkowski");
			list.Add("Arkadiusz Kaczmarek");
			break;
		case "Portugal":
			list.Add("Rafael Costa");
			list.Add("André Santos");
			list.Add("Francisco Pereira");
			list.Add("Diogo Fernandes");
			list.Add("Joaquim Silva");
			list.Add("Sérgio Rodrigues");
			list.Add("Rui Martins");
			list.Add("João Carvalho");
			list.Add("Pedro Almeida");
			list.Add("Luís Rodrigues");
			break;
		case "Romania":
			list.Add("Adrian Iacob");
			list.Add("Catalin Barbu");
			list.Add("Cristian Tudor");
			list.Add("Mihai Ungureanu");
			list.Add("Andrei Diaconu");
			list.Add("George Cosmin");
			list.Add("Marian Oprea");
			list.Add("Valeriu Moldoveanu");
			list.Add("Octavian Lazar");
			list.Add("Cosmin Dobre");
			break;
		case "Soviet Union":
			list.Add("Valentin Drozd");
			list.Add("Nikolai Gerasimovich Kuznetsov");
			list.Add("Amaury Lechapelain");
			list.Add("Yakov Fedorov");
			list.Add("Maksim Volkov");
			list.Add("Alexandr Popov");
			list.Add("Denis Kuznetsov");
			list.Add("Gennady Sokolov");
			list.Add("Fyodor Romanov");
			list.Add("Stepan Voronin");
			list.Add("Yaroslav Smirnov");
			list.Add("Artyom Ivanov");
			list.Add("Timur Petrov");
			break;
		case "Spain":
			list.Add("Fernando Torres");
			list.Add("Ricardo Jiménez");
			list.Add("Antonio Silva");
			list.Add("Rubén Ortega");
			list.Add("Alberto Rivas");
			list.Add("Óscar Mendoza");
			list.Add("Gonzalo Cruz");
			list.Add("Emilio Aguilar");
			list.Add("Roberto Gallego");
			list.Add("Héctor Peralta");
			break;
		case "Spanish Republic":
			list.Add("Luis González");
			list.Add("Ángel Prieto");
			list.Add("Francisco Blanco");
			list.Add("Pedro Cruz");
			list.Add("Antonio Soto");
			list.Add("Rafael Moya");
			list.Add("Juan Herrera");
			list.Add("Manuel Calvo");
			list.Add("Alberto Ortiz");
			list.Add("David Velasco");
			break;
		case "Sweden":
			list.Add("Mattias Nyström");
			list.Add("Oscar Johansson");
			list.Add("Sebastian Eklund");
			list.Add("Linus Hermansson");
			list.Add("Andreas Karlström");
			list.Add("Peter Gustavsson");
			list.Add("David Martinsson");
			list.Add("Joel Sundberg");
			list.Add("Robin Holmberg");
			list.Add("Pontus Wikström");
			break;
		case "Slovakia":
			list.Add("Jozef Kadlec");
			list.Add("Erik Beňo");
			list.Add("Dominik Hlaváč");
			list.Add("Jozef Jurčík");
			list.Add("Žigmund Bednár");
			list.Add("Slavomír Haluška");
			list.Add("Tibor Švec");
			list.Add("Branislav Babiak");
			list.Add("Rudolf Straka");
			list.Add("Norbert Žiak");
			break;
		case "Turkey":
			list.Add("Mehmet Çetin");
			list.Add("Ayhan Karadeniz");
			list.Add("İsmail Akın");
			list.Add("Murat Kılıç");
			list.Add("Emre Kara");
			list.Add("Cemal Demirci");
			list.Add("Musa Yılmaz");
			list.Add("Ahmet Şimşek");
			list.Add("Ali Yılmaz");
			list.Add("Erdem Özdemir");
			break;
		case "USA":
			list.Add("Anthony P. Damato");
			list.Add("John D. Bulkeley");
			list.Add("Robert E. Femoyer");
			list.Add("Samuel D. Dealey");
			list.Add("Samuel G. Fuqua");
			list.Add("Landon Evans");
			list.Add("Wyatt Reed");
			list.Add("Jaxon Bailey");
			list.Add("Gage Foster");
			list.Add("Miles Washington");
			list.Add("Eli Hughes");
			list.Add("Brayden Simmons");
			list.Add("Tanner Perry");
			list.Add("Micah Griffin");
			list.Add("Kai Foster");
			break;
		case "Yugoslavia":
			list.Add("Jovan Novak");
			list.Add("Dragan Ristić");
			list.Add("Slobodan Đukanović");
			list.Add("Branimir Milosavljević");
			list.Add("Zoran Petrović");
			list.Add("Vladislav Kovačević");
			list.Add("Nemanja Đorđević");
			list.Add("Mladen Stanković");
			list.Add("Viktor Mihajlović");
			list.Add("Nikola Milošević");
			break;
		}
		return list;
	}
}
