import java.util.Scanner;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.io.FileNotFoundException;

public class Program {
    public void main(String[] args) throws FileNotFoundException, IOException {
        System.out.println("Loading...");
        File file = new File("\\\\GMRDC1\\Folder Redirection\\Lorenzo.Lopez\\Documents\\LorenzoLopezComputerArchitecture\\JavaVirtualMachine\\textFile.txt");
        Scanner scanner = new Scanner(System.in);
        BufferedWriter writer = new BufferedWriter(new FileWriter(file));
        while(true)
        {
            System.out.println("Append:");
            writer.append(scanner.nextLine());
            writer.flush();
        }
    }
}