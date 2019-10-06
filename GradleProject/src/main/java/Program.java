import java.util.Scanner; 

public class Program {
    /*public class InnerClass
    {
        Class typeofThis = this.getClass();
        Class declaringClass = typeofThis.getDeclaringClass();
    }*/
    public void main(String[] args) throws java.io.FileNotFoundException{
        //InnerClass x = new InnerClass();
        java.io.File file = new java.io.File("\\\\GMRDC1\\Folder Redirection\\Lorenzo.Lopez\\Documents\\LorenzoLopezComputerArchitecture\\JavaVirtualMachine\\textFile.txt");
        //Scanner scanner = new Scanner(file);
        Scanner scanner = new Scanner(System.in);
        System.out.println("Initialized Scanner");
        
        while(true)
        {
            String read = scanner.nextLine();
            System.out.println(read);
        }
    }
}