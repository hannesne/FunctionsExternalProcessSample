#include <iostream>
#include <string>
#include "ExternalApp.h"

int main(int argc, char* argv[])
{
    std::cout << "Hello World!\n";
	std::cerr << "Sad Trombone...\n";
	if (argc > 1)
	{
		std::string argumentValue = argv[1];
		return std::stoi(argumentValue);
	}
	return 0;
}