import os
import pyodbc
import smtplib
from email.mime.text import MIMEText
import time

def enviar_email(destinatario, assunto, corpo, smtp_server, smtp_porta, smtp_usuario, smtp_senha, email_from):
    try:
        server = smtplib.SMTP(smtp_server, smtp_porta)
        server.starttls()
        server.login(smtp_usuario, smtp_senha)

        msg = MIMEText(corpo, 'html')
        msg['Subject'] = assunto
        msg['From'] = email_from
        msg['To'] = destinatario

        server.sendmail(email_from, [destinatario], msg.as_string())
        print(f"E-mail enviado para {destinatario}")
    except Exception as e:
        print(f"Erro ao enviar e-mail para {destinatario}: {e}")
    finally:
        if 'server' in locals() and server.noop()[0] == 250:
            server.quit()

def verificar_novos_registros(db_server, db_name, db_user, db_password):
    conn_str = (
        f'DRIVER=/opt/microsoft/msodbcsql17/lib64/libmsodbcsql-17.10.so.6.1;'
        f'SERVER={db_server},1433;'
        f'DATABASE={db_name};'
        f'UID={db_user};'
        f'PWD={db_password};'
    )
    try:
        cnxn = pyodbc.connect(conn_str)
        cursor = cnxn.cursor()
        cursor.execute("""
            SELECT ID_Usuario, Email, Nome 
            FROM Usuarios 
            WHERE email_boas_vindas_enviado = 0
        """)
        novos_usuarios = cursor.fetchall()
        return novos_usuarios, cnxn
    except Exception as e:
        print(f"Erro ao conectar ao banco de dados ou executar a query: {e}")
        return [], None

def marcar_email_enviado(usuario_id, cnxn):
    try:
        cursor = cnxn.cursor()
        cursor.execute("""
            UPDATE Usuarios 
            SET email_boas_vindas_enviado = 1 
            WHERE ID_Usuario = ?
        """, (usuario_id,))
        cnxn.commit()
    except Exception as e:
        print(f"Erro ao marcar email como enviado para o usuário {usuario_id}: {e}")

if __name__ == "__main__":
    db_server = os.environ.get("DB_SERVER")
    db_name = os.environ.get("DB_NAME")
    db_user = os.environ.get("DB_USER")
    db_password = os.environ.get("DB_PASSWORD")
    smtp_server = os.environ.get("SMTP_SERVER")
    smtp_porta = int(os.environ.get("SMTP_PORT", 587))
    smtp_usuario = os.environ.get("SMTP_USER")
    smtp_senha = os.environ.get("SMTP_PASSWORD")
    email_from = os.environ.get("EMAIL_FROM")

    novos_registros, conexao = verificar_novos_registros(db_server, db_name, db_user, db_password)

    if conexao:
        for usuario in novos_registros:
            usuario_id, email, nome = usuario
            assunto = "Bem-vindo(a) à nossa plataforma!"
            corpo = f"""
            <html>
            <body>
                <p>Olá, {nome}!</p>
                <p>Seja muito bem-vindo(a) ao LeapCert.</p>
                <p>Aproveite ao máximo todos os recursos que temos a oferecer!</p>
                <p>Atenciosamente,</p>
                <p>Equipe LeapCert</p>
            </body>
            </html>
            """
            enviar_email(email, assunto, corpo, smtp_server, smtp_porta, smtp_usuario, smtp_senha, email_from)
            marcar_email_enviado(usuario_id, conexao)
        conexao.close()
