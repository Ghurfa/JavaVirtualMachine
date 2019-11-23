import java.util.Scanner;
import java.net.*;
import java.io.*;


public class Program {
    public static native void ToggleDebugWrite(boolean value);
    public void main(String[] args) throws IOException{
        System.out.println("Loading...");
        Scanner scanner = new Scanner(System.in);
        ToggleDebugWrite(true);
        Socket clientSocket = new Socket("localhost", 9999);
    }
}