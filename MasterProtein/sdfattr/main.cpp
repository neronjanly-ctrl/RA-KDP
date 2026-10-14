#include <iostream>
#include <fstream>
#include <string.hpp>
using namespace std;

int main(int argc, char* argv[])
{
	if (argc < 2)
	{
		cout << "Usage: " << argv[0] << " <filename>" << endl;
		cout << "sdf Pubchem Id tool by ryan@imozo.cn" << endl;
		return 1;
	}

	int a = 0;
	ifstream file(argv[1]);
	bool first = true;

	for (string line; safe_getline(file, line);)
	{
		if (a == 0)
		{
			if (line == "> <PUBCHEM_SUBSTANCE_ID>")
			{
				if (first)
					first = false;
				else
					cout << endl;
				a = -1;
			}
			else if (line == "> <PUBCHEM_CID_ASSOCIATIONS>")
			{
				a = 1;
			}
		}
		else if (line.empty() || !isdigit(line[0]))
		{
			a = 0;
		}
		else
		{
			if (a > 1)
				cout << ',';
			if (a > 0)
				++a;
			for (char c : line)
				if (isdigit(c))
					cout << c;
				else
					break;
			if (a == -1)
				cout << ':';
		}
	}
	cout << endl;

	return 0;
}