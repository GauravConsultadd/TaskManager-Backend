pipeline {
    agent any
    
    stages {
        stage('Build') {
            steps {
                script {
                    // Clean and build the ASP.NET Core application
                    sh '/usr/local/share/dotnet/dotnet build TaskManager.csproj --configuration Release'
                }
            }
        }
        
        stage('Test') {
            steps {
                script {
                    // Run tests if needed
                    sh '/usr/local/share/dotnet/dotnet test TaskManager.Tests.csproj --configuration Release'
                }
            }
        }
        
        stage('Publish') {
            steps {
                script {
                    // Publish the ASP.NET Core application
                    sh '/usr/local/share/dotnet/dotnet publish TaskManager.csproj --configuration Release --output publish_output'
                }
            }
        }
    }
}